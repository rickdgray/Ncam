﻿using System.CommandLine.Invocation;

namespace Ncam.Commands
{
    public class NcamCommandHandler : ICommandHandler
    {
        public int Invoke(InvocationContext context)
        {
            throw new NotImplementedException();
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
