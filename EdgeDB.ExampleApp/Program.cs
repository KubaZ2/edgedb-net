﻿using EdgeDB.Models;
using EdgeDB;
using System.Net.Sockets;
using System.Net.Security;
using Newtonsoft.Json;
using EdgeDB.Codecs;
using Test;
using System.Linq.Expressions;
using EdgeDB.Utils;

Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);

var edgedb = new EdgeDBClient(EdgeDBConnection.FromProjectFile(@"../../../edgedb.toml"), new EdgeDBConfig
{
    Logger = Logger.GetLogger<EdgeDBClient>()
});

var result = await edgedb.QueryAsync<Person>(x => true);

await Task.Delay(-1);

public class Person
{
    [EdgeDBProperty("id")]
    public Guid Id { get; set; }

    [EdgeDBProperty("name")]
    public string? Name { get; set; }

    [EdgeDBProperty("email")]
    public string? Email { get; set; }
}