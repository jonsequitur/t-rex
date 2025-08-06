// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace TRexLib.InteractiveExtension;

internal class RunTestsCommand : KernelDirectiveCommand
{
    private FileSystemInfo project;

    public FileSystemInfo Project
    {
        get => project ??= new DirectoryInfo(Directory.GetCurrentDirectory());
        set => project = value;
    }

    public override IEnumerable<string> GetValidationErrors(CompositeKernel kernel)
    {
        if (!Project.Exists)
        {
            yield return $"The specified file or directory ({Project.FullName}) does not exist.";
        }
    }
}