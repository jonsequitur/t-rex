// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace TRexLib.InteractiveExtension;

internal class ShowTestsCommand : KernelDirectiveCommand
{
    private DirectoryInfo dir;

    public DirectoryInfo Dir
    {
        get => dir ??= new DirectoryInfo(Directory.GetCurrentDirectory());
        set => dir = value;
    }

    public override IEnumerable<string> GetValidationErrors(CompositeKernel kernel)
    {
        if (!Dir.Exists)
        {
            yield return "The specified directory does not exist.";
        }
    }
}