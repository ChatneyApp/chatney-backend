using HotChocolate.Execution.Configuration;

interface IDomainSetup
{
    public IRequestExecutorBuilder Setup(IRequestExecutorBuilder builder);
}