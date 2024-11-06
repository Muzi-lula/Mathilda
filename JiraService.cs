﻿using Atlassian.Jira;
using Mathilda.Models;

namespace Mathilda;

public interface ITicketService
{
    public Task<TicketsResponse> GetInProgressTickets(DateTime start, DateTime end);
}

public class JiraService : ITicketService
{
    private const string jqlCurrentUserActiveTickets = "assignee = currentuser() AND status WAS \"In Progress\" DURING (\"2024-10-01\", \"2024-11-05\")";
    public async Task<TicketsResponse> GetInProgressTickets(DateTime start, DateTime end)
    {
        // create a connection to JIRA using the Rest client
        var jira = Jira.CreateRestClient("https://company.atlassian.net/", "EMAIL/USERNAMR", "API-TOKEN");

        var response = new TicketsResponse()
        {
            TicketsOnDays = new List<TicketsOnDay>()
        };
        
        for (DateTime day = start; day <= end; day = day.AddDays(1))
        {
            var issuesOnDay = jira.Issues.GetIssuesFromJqlAsync(
                new IssueSearchOptions(
                    $"assignee = currentuser() AND status WAS \"In Progress\" ON (\"{day:yyyy-MM-dd}\")"));

            var ticketsOnDay = issuesOnDay.Result.Select(x => new TicketInfo()
            {
                Summary = x.Summary,
                TicketKey = x.Key.Value,
                Type = x.Type.Name,
            }).ToList();
            
            response.TicketsOnDays.Add(new TicketsOnDay()
            {
                Date = day,
                Tickets = ticketsOnDay
            });
        }
        
        return response;
    }
}