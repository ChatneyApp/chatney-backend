using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Infra;
using ChatneyBackend.Tests.Support;
using HotChocolate;

namespace ChatneyBackend.Tests.Domains.Messages;

public class MessageMutationQueriesTests
{
  [Fact]
  public async Task IncrementChildrenCountAsync_IncrementsParentCount()
  {
    var context = new MessageMutationsTestContext();
    var parent = context.SeedMessage("parent", id: 1, childrenCount: 2);

    var childrenCount = await MessageMutationQueries.IncrementChildrenCountAsync(
      context.MessagesRepo,
      parent.Id);

    Assert.Equal(3, childrenCount);
    Assert.Equal(3, context.MessagesRepo.Items.Single().ChildrenCount);
  }

  [Fact]
  public async Task DecrementChildrenCountAsync_DoesNotGoBelowZero()
  {
    var context = new MessageMutationsTestContext();
    context.SeedMessage("parent", id: 1, childrenCount: 0);

    var childrenCount = await MessageMutationQueries.DecrementChildrenCountAsync(
      context.MessagesRepo,
      1);

    Assert.Equal(0, childrenCount);
  }

  [Fact]
  public async Task UpdateMessageAsync_UpdatesStoredMessage()
  {
    var context = new MessageMutationsTestContext();
    var message = context.SeedMessage("old content", id: 1);

    var updatedAt = await MessageMutationQueries.UpdateMessageAsync(
      context.MessagesRepo,
      message.Id,
      "new content",
      [10],
      [20]);

    Assert.NotNull(updatedAt);
    var stored = context.MessagesRepo.Items.Single();
    Assert.Equal("new content", stored.Content);
    Assert.Equal([10], stored.AttachmentIds);
    Assert.Equal([20], stored.UrlPreviewIds);
  }

  [Fact]
  public async Task InsertReactionAsync_IsIdempotent()
  {
    var context = new MessageMutationsTestContext();
    var userId = context.User.Id;

    await MessageMutationQueries.InsertReactionAsync(context.ReactionsRepo, 1, userId, "thumbsup");
    await MessageMutationQueries.InsertReactionAsync(context.ReactionsRepo, 1, userId, "thumbsup");

    Assert.Single(context.ReactionsRepo.Items);
  }

  [Fact]
  public async Task DeleteReactionAsync_RemovesExistingReaction()
  {
    var context = new MessageMutationsTestContext();
    await MessageMutationQueries.InsertReactionAsync(
      context.ReactionsRepo,
      1,
      context.User.Id,
      "thumbsup");

    var deleted = await MessageMutationQueries.DeleteReactionAsync(
      context.ReactionsRepo,
      1,
      context.User.Id,
      "thumbsup");

    Assert.Equal(1, deleted);
    Assert.Empty(context.ReactionsRepo.Items);
  }
}

public class MessageMutationsTests
{
  [Fact]
  public async Task AddMessage_CreatesMessageAndBroadcasts()
  {
    var context = new MessageMutationsTestContext();
    var dto = new MessageDto
    {
      ChannelId = context.Channel.Id,
      Content = "hello world",
    };

    var result = await context.Mutations.AddMessage(
      context.Repos,
      context.Resolver,
      context.Principal,
      dto,
      context.WebSocket);

    Assert.NotNull(result);
    Assert.Equal("hello world", result.Content);
    Assert.Single(context.MessagesRepo.Items);
    Assert.Single(context.WebSocket.NewMessages);
    Assert.Equal(result.Id, context.WebSocket.NewMessages[0].Message.Id);
  }

  [Fact]
  public async Task AddMessage_ThrowsWhenChannelMissing()
  {
    var context = new MessageMutationsTestContext();
    var dto = new MessageDto
    {
      ChannelId = 999,
      Content = "hello",
    };

    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      context.Mutations.AddMessage(
        context.Repos,
        context.Resolver,
        context.Principal,
        dto,
        context.WebSocket));
  }

  [Fact]
  public async Task AddMessage_ThrowsWhenCreatePermissionMissing()
  {
    var context = new MessageMutationsTestContext([Permission.ChannelReadMessage]);
    var dto = new MessageDto
    {
      ChannelId = context.Channel.Id,
      Content = "hello",
    };

    var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
      context.Mutations.AddMessage(
        context.Repos,
        context.Resolver,
        context.Principal,
        dto,
        context.WebSocket));

    Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(exception.Errors).Code);
  }

  [Fact]
  public async Task AddMessage_WithParent_IncrementsChildrenCount()
  {
    var context = new MessageMutationsTestContext();
    var parent = context.SeedMessage("parent", id: 1, childrenCount: 1);

    var dto = new MessageDto
    {
      ChannelId = context.Channel.Id,
      Content = "reply",
      ParentId = parent.Id,
    };

    await context.Mutations.AddMessage(
      context.Repos,
      context.Resolver,
      context.Principal,
      dto,
      context.WebSocket);

    Assert.Equal(2, context.MessagesRepo.Items.Single(item => item.Id == parent.Id).ChildrenCount);
    Assert.Single(context.WebSocket.ChildrenCountUpdates);
    Assert.Equal(parent.Id, context.WebSocket.ChildrenCountUpdates[0].MessageId);
    Assert.Equal(2, context.WebSocket.ChildrenCountUpdates[0].ChildrenCount);
  }

  [Fact]
  public async Task UpdateMessage_ReturnsFalseWhenMessageMissing()
  {
    var context = new MessageMutationsTestContext();

    await Assert.ThrowsAsync<GraphQLException>(() =>
      context.Mutations.UpdateMessage(
        context.Repos,
        context.Resolver,
        context.Principal,
        context.WebSocket,
        new MessageUpdateDto { Id = 404, Content = "updated" }));
  }

  [Fact]
  public async Task UpdateMessage_ThrowsForbiddenWithoutPermission()
  {
    var context = new MessageMutationsTestContext([Permission.ChannelReadMessage]);
    var message = context.SeedMessage(id: 1);

    await Assert.ThrowsAsync<GraphQLException>(() =>
      context.Mutations.UpdateMessage(
        context.Repos,
        context.Resolver,
        context.Principal,
        context.WebSocket,
        new MessageUpdateDto { Id = message.Id, Content = "updated" }));
  }

  [Fact]
  public async Task UpdateMessage_AllowsOwnerToEditOwnMessage()
  {
    var context = new MessageMutationsTestContext([Permission.ChannelEditOwnMessage]);
    var message = context.SeedMessage("before", id: 1);

    var updated = await context.Mutations.UpdateMessage(
      context.Repos,
      context.Resolver,
      context.Principal,
      context.WebSocket,
      new MessageUpdateDto { Id = message.Id, Content = "after", AttachmentIds = [5] });

    Assert.True(updated);
    Assert.Equal("after", context.MessagesRepo.Items.Single().Content);
    Assert.Single(context.WebSocket.EditedMessages);
  }

  [Fact]
  public async Task UpdateMessage_UsesExistingUrlPreviewsWhenContentUnchanged()
  {
    var context = new MessageMutationsTestContext();
    var message = context.SeedMessage("same", id: 1);
    message.UrlPreviewIds = [7];
    await context.MessagesRepo.UpdateOne(message);

    var updated = await context.Mutations.UpdateMessage(
      context.Repos,
      context.Resolver,
      context.Principal,
      context.WebSocket,
      new MessageUpdateDto { Id = message.Id, Content = "same" });

    Assert.True(updated);
    Assert.Equal([7], context.MessagesRepo.Items.Single().UrlPreviewIds);
  }

  [Fact]
  public async Task DeleteMessage_ThrowsNotFoundWhenMessageMissing()
  {
    var context = new MessageMutationsTestContext();

    await Assert.ThrowsAsync<GraphQLException>(() =>
      context.Mutations.DeleteMessage(
        context.WebSocket,
        context.Repos,
        context.Resolver,
        context.Principal,
        404));
  }

  [Fact]
  public async Task DeleteMessage_ThrowsForbiddenWithoutPermission()
  {
    var context = new MessageMutationsTestContext([Permission.ChannelReadMessage]);
    var message = context.SeedMessage(id: 1);

    await Assert.ThrowsAsync<GraphQLException>(() =>
      context.Mutations.DeleteMessage(
        context.WebSocket,
        context.Repos,
        context.Resolver,
        context.Principal,
        message.Id));
  }

  [Fact]
  public async Task DeleteMessage_RemovesThreadWhenDeletingTopLevelMessage()
  {
    var context = new MessageMutationsTestContext();
    var parent = context.SeedMessage("parent", id: 1);
    context.SeedMessage("child", parentId: parent.Id, id: 2);

    var deleted = await context.Mutations.DeleteMessage(
      context.WebSocket,
      context.Repos,
      context.Resolver,
      context.Principal,
      parent.Id);

    Assert.True(deleted);
    Assert.Empty(context.MessagesRepo.Items);
    Assert.Single(context.WebSocket.DeletedMessages);
  }

  [Fact]
  public async Task DeleteMessage_DecrementsParentChildrenCountForReplies()
  {
    var context = new MessageMutationsTestContext();
    var parent = context.SeedMessage("parent", id: 1, childrenCount: 2);
    var reply = context.SeedMessage("reply", parentId: parent.Id, id: 2);

    var deleted = await context.Mutations.DeleteMessage(
      context.WebSocket,
      context.Repos,
      context.Resolver,
      context.Principal,
      reply.Id);

    Assert.True(deleted);
    Assert.Single(context.MessagesRepo.Items);
    Assert.Equal(1, context.MessagesRepo.Items.Single().ChildrenCount);
    Assert.Single(context.WebSocket.ChildrenCountUpdates);
    Assert.Equal(1, context.WebSocket.ChildrenCountUpdates[0].ChildrenCount);
  }

  [Fact]
  public async Task AddReaction_ReturnsErrorForMissingMessage()
  {
    var context = new MessageMutationsTestContext();

    var result = await context.Mutations.AddReaction(
      context.WebSocket,
      context.Repos,
      "thumbsup",
      404,
      context.Principal);

    Assert.Equal("error", result.status);
    Assert.Equal("wrong message id", result.message);
  }

  [Fact]
  public async Task AddReaction_BroadcastsOnSuccess()
  {
    var context = new MessageMutationsTestContext();
    var message = context.SeedMessage(id: 1);

    var result = await context.Mutations.AddReaction(
      context.WebSocket,
      context.Repos,
      "thumbsup",
      message.Id,
      context.Principal);

    Assert.Equal("success", result.status);
    Assert.Single(context.ReactionsRepo.Items);
    Assert.Single(context.WebSocket.AddedReactions);
    Assert.Equal("thumbsup", context.WebSocket.AddedReactions[0].Code);
  }

  [Fact]
  public async Task DeleteReaction_ReturnsErrorWhenReactionMissing()
  {
    var context = new MessageMutationsTestContext();
    var message = context.SeedMessage(id: 1);

    var result = await context.Mutations.DeleteReaction(
      context.WebSocket,
      context.Repos,
      "thumbsup",
      message.Id,
      context.Principal);

    Assert.Equal("error", result.status);
    Assert.Equal("reaction not found", result.message);
  }

  [Fact]
  public async Task DeleteReaction_BroadcastsOnSuccess()
  {
    var context = new MessageMutationsTestContext();
    var message = context.SeedMessage(id: 1);
    await MessageMutationQueries.InsertReactionAsync(
      context.ReactionsRepo,
      message.Id,
      context.User.Id,
      "thumbsup");

    var result = await context.Mutations.DeleteReaction(
      context.WebSocket,
      context.Repos,
      "thumbsup",
      message.Id,
      context.Principal);

    Assert.Equal("success", result.status);
    Assert.Empty(context.ReactionsRepo.Items);
    Assert.Single(context.WebSocket.DeletedReactions);
  }
}
