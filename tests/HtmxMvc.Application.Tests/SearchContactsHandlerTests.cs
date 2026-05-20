using HtmxMvc.Application.Contacts;
using HtmxMvc.Domain;
using Xunit;

namespace HtmxMvc.Application.Tests;

public class SearchContactsHandlerTests
{
    private static FakeContactRepository SeededRepo() => new(
        new Contact { Name = "Ada Lovelace",     Email = "ada@analyticalengine.org",  Phone = "555-0101" },
        new Contact { Name = "Alan Turing",      Email = "alan@bletchley.uk",         Phone = "555-0102" },
        new Contact { Name = "Grace Hopper",     Email = "grace@navy.mil",            Phone = "555-0103" });

    [Fact]
    public async Task Returns_all_when_query_is_null()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync(null);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Returns_all_when_query_is_whitespace()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("   ");
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Matches_name_case_insensitively()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("ADA");
        Assert.Single(result);
        Assert.Equal("Ada Lovelace", result[0].Name);
    }

    [Fact]
    public async Task Matches_email_substring()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("bletchley");
        Assert.Single(result);
        Assert.Equal("Alan Turing", result[0].Name);
    }

    [Fact]
    public async Task Matches_phone_substring()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("0103");
        Assert.Single(result);
        Assert.Equal("Grace Hopper", result[0].Name);
    }

    [Fact]
    public async Task Returns_empty_when_no_match()
    {
        var handler = new SearchContactsHandler(SeededRepo());
        var result = await handler.ExecuteAsync("zzz");
        Assert.Empty(result);
    }
}
