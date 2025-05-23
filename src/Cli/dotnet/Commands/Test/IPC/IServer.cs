﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Cli.Commands.Test.IPC;

internal interface IServer : INamedPipeBase,
#if NETCOREAPP
IAsyncDisposable,
#endif
IDisposable
{
    PipeNameDescription PipeName { get; }

    Task WaitConnectionAsync(CancellationToken cancellationToken);
}
