using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.InstallWizard;

public static class SeedMessages
{
    private const string Admin = "admin";
    private const string Test1 = "test1";
    private const string Test2 = "test2";
    private const string Emma = "emma_clarke";
    private const string Liam = "liam_novak";
    private const string Sofia = "sofia_marino";
    private const string Daniel = "daniel_weber";
    private const string Priya = "priya_nair";

    /// <summary>Seed accounts besides admin and test1/test2: nickname, full name and bio.</summary>
    public static readonly (string Nickname, string FullName, string Bio)[] NamedUsers =
    [
        (Emma, "Emma Clarke",
            "Product designer. I care about onboarding flows, empty states and readable error messages.\n\n" +
            "Usually online 9:00-17:30 UK time. Outside of work: pottery and long walks with my dog Biscuit."),
        (Liam, "Liam Novak",
            "Backend developer on the reporting team. Queues, exports and everything that runs at night.\n" +
            "Ping me about report-export-worker.\n\n" +
            "Amateur climber, professional coffee drinker ☕"),
        (Sofia, "Sofia Marino",
            "QA engineer. If it's broken on staging, I probably found it first.\n\n" +
            "Bug reports welcome in any form, but steps to reproduce make my day.\n" +
            "Based in Milan, CET."),
        (Daniel, "Daniel Weber",
            "DevOps / infrastructure. Deploys, monitoring, alerts and the occasional 3am page.\n\n" +
            "For anything urgent on prod, call instead of messaging.\n" +
            "Berlin. Cyclist, home-lab tinkerer."),
        (Priya, "Priya Nair",
            "Engineering manager. Here to unblock people and keep the incident log honest.\n\n" +
            "1:1 slots are open on Tuesdays and Thursdays, just pick a time in my calendar."),
    ];

    /// <param name="Gap">Minutes since the previous message of the same list (for the first thread reply: since the thread root).</param>
    /// <param name="Reactions">"code:nickname,nickname;code:nickname".</param>
    private sealed record Line(int Gap, string Author, string Content, string Reactions = "", Line[]? Thread = null);

    private static readonly Line[] StagingThread =
    [
        new(1, Daniel, "What exactly is slow? Every page or something specific?"),
        new(1, Sofia, "Everything. Login took about 8 seconds, the dashboard even longer."),
        new(1, Test1, "Search requests time out completely for me"),
        new(1, Daniel, "Ok, CPU on the app server is at 100%. Checking what's eating it.", $"eyes:{Sofia}"),
        new(1, Liam, "Could it be the deploy from last night? I merged the report export changes around 22:00"),
        new(1, Daniel, "Possible. Which job does the export use?"),
        new(1, Liam, "report-export-worker, it runs from the queue"),
        new(2, Daniel, "Yep, that's the one. 14 instances of it running right now 😬", $"sweat_smile:{Liam},{Test1}"),
        new(1, Liam, "14?! There should be one at most"),
        new(1, Admin, "Do we have a concurrency limit on that queue?"),
        new(1, Liam, "We do on prod. On staging the config was never updated, my bad"),
        new(1, Daniel, "Also looks like every job retries forever when it fails. They've been restarting each other since midnight."),
        new(1, Sofia, "That matches, the nightly tests started failing around 00:30"),
        new(1, Test2, "Is it safe to kill them? I have a test export somewhere in that queue"),
        new(1, Daniel, "Your export will be lost, you'd need to start it again. Ok with that?"),
        new(1, Test2, "Sure, it was just test data", $"thumbs_up:{Daniel}"),
        new(1, Daniel, "Killing the workers and clearing the queue now."),
        new(2, Daniel, "Done. CPU is down to 12%.", $"party_popper:{Sofia},{Test1};raised_hands:{Liam}"),
        new(1, Sofia, "Login is instant again 🎉"),
        new(1, Test1, "Search works too, thanks!"),
        new(1, Admin, "Nice work. Liam, can you fix the retry logic so this doesn't happen again?"),
        new(1, Liam, "Already on it. Max 3 retries with a backoff, and I'll add the concurrency limit to the staging config.", $"thumbs_up:{Admin},{Daniel}"),
        new(2, Priya, "Does this affect production in any way?"),
        new(1, Daniel, "No, prod has the limit and the export feature isn't enabled there yet."),
        new(1, Priya, "Good. Please add a short note to the incident log anyway."),
        new(1, Daniel, "Will do."),
        new(3, Emma, "Late to the party, but can we get an alert for this? I lost half an hour thinking my wifi was broken 😅", $"joy:{Sofia},{Daniel},{Test1}"),
        new(1, Daniel, "Fair. I'll add a CPU alert for staging, we only have them for prod right now.", $"hundred:{Emma},{Sofia}"),
        new(9, Liam, "Fix is up for review: retry limit + staging config."),
        new(4, Admin, "Approved and merged. Thanks everyone.", $"check:{Liam},{Daniel};clap:{Priya}"),
    ];

    private static readonly Line[] AvatarThread =
    [
        new(3, Admin, "Known, it crops to a square from the top-left corner. Fix is planned for the next release."),
        new(1, Emma, "I have a design for a proper crop dialog, it just didn't make it into this sprint.", $"thumbs_up:{Test2},{Admin}"),
        new(1, Test2, "Ok, I'll just use a square picture for now"),
    ];

    private static readonly Line[] DatesThread =
    [
        new(2, Test2, "We used date-fns in the last project, no complaints"),
        new(1, Test1, "Does it handle time zones?"),
        new(2, Test2, "With the date-fns-tz add-on, yes"),
        new(2, Liam, "Day.js is another option, it's tiny and the API is pretty much moment.js", $"thumbs_up:{Test1}"),
        new(1, Test1, "Bundle size matters, the search page is already heavy"),
        new(3, Admin, "Just store everything in UTC and convert when you display it. Saves a lot of pain.", $"hundred:{Liam},{Daniel}"),
        new(1, Test1, "That's the plan, the problem is the input - users type dates in whatever format they like"),
        new(2, Emma, "What does the field look like now? A plain text input?"),
        new(1, Test1, "Yes, free text. People type things like \"3 march\", \"03/04/25\", \"yesterday\"..."),
        new(1, Sofia, "03/04/25 is my favourite. March 4th or April 3rd? Depends on who you ask 🙃", $"joy:{Test1},{Emma},{Liam}"),
        new(2, Emma, "Then give them a date picker and keep the text input only as a shortcut."),
        new(1, Priya, "+1 for the picker. Support gets a ticket about wrong dates at least once a week."),
        new(2, Test1, "Ok, picker it is. Do we have one in the design system?"),
        new(1, Emma, "We do, it's used on the reports page. I'll send you the specs.", $"pray:{Test1}"),
        new(3, Daniel, "Whatever you do, please log the raw input somewhere. Debugging date bugs without it is no fun."),
        new(1, Test1, "Good idea, will do"),
        new(2, Liam, "The API already accepts ISO 8601, so nothing changes on the backend if the picker sends that."),
        new(1, Test1, "Perfect. So: picker + ISO to the API + date-fns only for display.", $"thumbs_up:{Emma},{Liam},{Priya},{Admin}"),
        new(2, Test2, "Sounds good. I can share our formatting helpers from the old project."),
        new(1, Test1, "Please do. Thanks everyone, that was faster than googling 😄", $"heart:{Emma};joy:{Test2}"),
    ];

    private static readonly Line[] Public1Conversation =
    [
        new(0, Admin, "Morning everyone 👋 Welcome to the new chat, this is the main public channel.", $"wave:{Test1},{Test2},{Emma},{Priya};party_popper:{Liam},{Sofia}"),
        new(2, Test1, "Morning! Finally, no more email threads", $"hundred:{Daniel},{Liam};joy:{Emma}"),
        new(1, Emma, "Hi all! Love the dark theme already 😍", $"heart:{Admin}"),
        new(1, Test2, "Hey! Looks nice. Is this the place for general stuff?"),
        new(3, Admin, "Yep, anything that doesn't fit a more specific channel goes here.", $"thumbs_up:{Test2},{Priya}"),
        new(6, Priya, "Good morning! Reminder that the demo is on Thursday at 15:00, the invite goes out after lunch.", $"thumbs_up:{Test1},{Test2},{Sofia},{Liam}"),
        new(2, Test1, "Great, I need about 10 minutes for the search part"),
        new(3, Test2, "I can show the new upload flow if there's time left"),
        new(1, Priya, "There will be, the slot is an hour long. Emma, do you want to show the new onboarding screens?"),
        new(2, Emma, "Sure, they're almost done. Two screens left to polish.", $"fire:{Priya};clap:{Admin}"),
        new(31, Sofia, "Has anyone seen the staging server today? It's really slow for me", $"eyes:{Daniel},{Admin}", StagingThread),
        new(2, Test1, "Same here, pages take like 5 seconds to load"),
        new(1, Daniel, "Looking into it, let's keep the details in the thread 👆", $"pray:{Sofia},{Test1}"),
        new(24, Daniel, "Staging is back to normal. A stuck background job was eating all the CPU, details are in the thread.", $"raised_hands:{Sofia},{Test1},{Test2},{Priya};rocket:{Liam}"),
        new(1, Sofia, "Much better, thanks!"),
        new(41, Liam, "Lunch? I'm thinking about that ramen place around the corner", $"thumbs_up:{Test2},{Emma}"),
        new(2, Test2, "I'm in. 12:30?"),
        new(1, Emma, "Me too!"),
        new(3, Admin, "Can't today, call at 12:45. Bring me back a coffee? 🙂", $"coffee:{Liam}"),
        new(1, Liam, "Sure, the usual flat white?"),
        new(1, Admin, "You know me too well.", $"joy:{Liam},{Emma}"),
        new(2, Priya, "I brought lunch from home, have fun. Send pictures 🍜"),
        new(78, Test2, "Back. The ramen was great, 10/10 would recommend", $"fire:{Liam},{Emma}"),
        new(1, Liam, "And the queue was only 20 minutes, a new record", $"sweat_smile:{Priya},{Sofia}"),
        new(2, Admin, "Thanks for the coffee!", $"heart:{Liam}"),
        new(16, Test2, "Small thing: the avatar upload cuts my picture weirdly. Is that known?", "", AvatarThread),
        new(2, Test1, "That explains why I only have a forehead on mine 😅", $"joy:{Test2},{Emma},{Sofia},{Daniel};rofl:{Liam}"),
        new(1, Daniel, "lol I thought that was on purpose"),
        new(38, Priya, "Reminder: please update your tasks before the end of the day, I'm putting together the weekly report.", $"thumbs_up:{Test1},{Liam},{Emma};check:{Sofia}"),
        new(4, Test1, "Done"),
        new(9, Test2, "Updated mine too. I moved the export task to next week, it's blocked by the API changes."),
        new(2, Liam, "The API part should be merged tomorrow, I'll ping you.", $"thumbs_up:{Test2}"),
        new(27, Test1, "Does anybody know a good library for parsing dates in different formats?", $"thinking:{Liam},{Test2}", DatesThread),
        new(3, Emma, "Whatever you pick, please don't make users type dates by hand 🙏", $"hundred:{Admin},{Sofia}"),
        new(22, Sofia, "Found two bugs in the new upload flow, tickets are in the tracker. Nothing blocking for the demo.", $"eyes:{Test2};pray:{Priya}"),
        new(2, Test2, "Thanks, I'll fix them tomorrow morning"),
        new(15, Emma, "Onboarding screens are done! Link is in the design channel, feedback welcome.", $"party_popper:{Priya},{Admin},{Test1};heart:{Sofia},{Liam};fire:{Test2}"),
        new(3, Priya, "They look great. The illustration on the second screen is my favourite."),
        new(1, Emma, "Thank you 😊"),
        new(19, Daniel, "Heads up: I'm rotating the staging certificates at 17:00. Expect a minute of downtime.", $"thumbs_up:{Admin},{Liam},{Sofia},{Test1}"),
        new(2, Admin, "Thanks for the warning."),
        new(24, Daniel, "Certificates rotated, everything is back up.", $"check:{Admin},{Priya};rocket:{Liam}"),
        new(12, Test2, "I'm heading out a bit early today, dentist appointment", $"crying:{Emma};flexed_biceps:{Liam}"),
        new(1, Test1, "Good luck!"),
        new(2, Admin, "No problem. See you tomorrow."),
        new(21, Test1, "Pushed the search changes, would be nice if someone could take a look tomorrow morning", $"eyes:{Liam}"),
        new(3, Liam, "I'll review it first thing.", $"pray:{Test1}"),
        new(4, Sofia, "I'll run the regression suite on it after the review."),
        new(2, Priya, "Great work today everyone. Demo prep sync tomorrow at 10:00.", $"thumbs_up:{Test1},{Emma},{Liam},{Daniel},{Sofia};clap:{Admin}"),
        new(1, Test1, "Thanks! Have a good evening everyone 👋", $"wave:{Emma},{Priya},{Admin},{Sofia}"),
    ];

    /// <summary>Fills the channel with a day of conversation: messages, threads and reactions.</summary>
    public static async Task SeedPublic1(
        AppRepos repos,
        int channelId,
        IReadOnlyDictionary<string, Guid> userIdsByNickname,
        DateTime now)
    {
        var startedAt = now.AddMinutes(-Public1Conversation.Sum(line => line.Gap) - 5);
        var roots = BuildMessages(Public1Conversation, startedAt, channelId, null, userIdsByNickname);
        await repos.Messages.InsertBulk(roots.Select(root => root.Message).ToList());

        var replies = roots
            .Where(root => root.Line.Thread != null)
            .SelectMany(root => BuildMessages(
                root.Line.Thread!, root.Message.CreatedAt, channelId, root.Message.Id, userIdsByNickname))
            .ToList();
        await repos.Messages.InsertBulk(replies.Select(reply => reply.Message).ToList());

        var reactions = roots.Concat(replies)
            .SelectMany(item => ParseReactions(item.Line.Reactions)
                .Select(reaction => new MessageReaction
                {
                    MessageId = item.Message.Id,
                    UserId = userIdsByNickname[reaction.Nickname],
                    Code = reaction.Code,
                }))
            .ToList();
        await repos.Reactions.InsertBulk(reactions);

        // InsertBulk stamps updated_at with the current time, which would show every seeded message as edited
        await repos.Messages.ExecuteAsync(
            $"UPDATE {DomainSettings.MessageTableName} SET updated_at = created_at WHERE channel_id = @ChannelId",
            new { ChannelId = channelId });
    }

    private static List<(Line Line, Message Message)> BuildMessages(
        Line[] lines,
        DateTime startedAt,
        int channelId,
        int? parentId,
        IReadOnlyDictionary<string, Guid> userIdsByNickname)
    {
        var createdAt = startedAt;
        var result = new List<(Line, Message)>();

        foreach (var line in lines)
        {
            createdAt = createdAt.AddMinutes(line.Gap);
            result.Add((line, new Message
            {
                ChannelId = channelId,
                UserId = userIdsByNickname[line.Author],
                Content = line.Content,
                Status = "sent",
                ParentId = parentId,
                ChildrenCount = line.Thread?.Length ?? 0,
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
            }));
        }

        return result;
    }

    private static IEnumerable<(string Code, string Nickname)> ParseReactions(string reactions) =>
        reactions
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(group => group.Split(':'))
            .SelectMany(parts => parts[1].Split(',').Select(nickname => (parts[0], nickname)));
}
