﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.DotNet.Cli.Installer.Windows;

/// <summary>
/// Describes the various actions associated with an MSI.
/// </summary>
public enum InstallAction
{
    None,
    Install,
    Uninstall,
    Repair,
}
