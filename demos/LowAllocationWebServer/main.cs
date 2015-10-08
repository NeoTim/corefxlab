// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Console.WriteLine("Sample Rest Server Started");
        Console.WriteLine("The server implements /time REST method.");
        Console.WriteLine("Browse to http://localhost:9999/time to test it.\n");

        var log = new ConsoleLog((Log.Level.Off));
        var restServer = new SampleRestServer(log, 8080, 0, 0, 0, 0); 
        restServer.StartAsync();

        Console.WriteLine("Press ENTER to exit ...");
        Console.ReadLine();

        restServer.Stop();
    }
}

