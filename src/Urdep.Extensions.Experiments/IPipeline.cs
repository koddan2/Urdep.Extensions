namespace Urdep.Extensions.Experiments;

public delegate Task NextStepAsync<T>(T context);

public interface IPipelineStep<T>
{
    Task InvokeAsync(NextStepAsync<T> next, T context);
}

public interface IPipeline<T>
{
    void Add(IPipelineStep<T> step);

    Task<T> InvokeAsync(T context);
}

public class Pipeline<T> : IPipeline<T>
{
    private readonly List<IPipelineStep<T>> _steps = [];

    void IPipeline<T>.Add(IPipelineStep<T> step)
    {
        _steps.Add(step);
    }

    async Task<T> IPipeline<T>.InvokeAsync(T context)
    {
        await _steps[0].InvokeAsync(GetNextStep(0), context);

        return context;
    }

    NextStepAsync<T> GetNextStep(int index)
    {
        if (index < _steps.Count - 1)
        {
            return (context) => _steps[index + 1].InvokeAsync(GetNextStep(index + 1), context);
        }

        return (_) => Task.CompletedTask;
    }
}

public static class Examples
{
    public record Data
    {
        public int Counter { get; set; }
    }

    public class Step1 : IPipelineStep<Data>
    {
        public async Task InvokeAsync(NextStepAsync<Data> next, Data context)
        {
            context.Counter++;
            await next(context);
            context.Counter++;
        }
    }

    public class Step2 : IPipelineStep<Data>
    {
        public async Task InvokeAsync(NextStepAsync<Data> next, Data context)
        {
            context.Counter++;
            await next(context);
            context.Counter++;
        }
    }

    public class Step3 : IPipelineStep<Data>
    {
        public async Task InvokeAsync(NextStepAsync<Data> next, Data context)
        {
            context.Counter++;
            await next(context);
            await Task.Delay(1);
            context.Counter++;
        }
    }

    public static async Task Example1()
    {
        IPipeline<Data> pipeline = new Pipeline<Data>();
        pipeline.Add(new Step1());
        pipeline.Add(new Step2());
        pipeline.Add(new Step3());

        await pipeline.InvokeAsync(new());
    }
}
