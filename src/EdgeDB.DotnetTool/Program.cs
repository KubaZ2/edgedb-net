﻿using CommandLine;
using EdgeDB.DotnetTool;
using System.Reflection;

var commands = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

Parser.Default.ParseArguments(args, commands.ToArray()).WithParsed<ICommand>(t => t.Execute());