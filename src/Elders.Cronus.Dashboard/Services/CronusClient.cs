﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus.Dashboard.Models
{
    public class CronusClient : HttpClientBase
    {
        private readonly TokenClient token;
        private readonly ILogger<CronusClient> log;

        public CronusClient(HttpClient client, TokenClient token, ILogger<CronusClient> log) : base(client)
        {
            this.token = token;
            this.log = log;
        }

        public async Task<Response<ProjectionCollection>> GetProjectionsAsync(Connection connection)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, connection.CronusEndpoint + "/projections");
            if (string.IsNullOrEmpty(connection.oAuth.ServerEndpoint) == false)
            {
                var accessToken = await token.GetAccessTokenAsync(connection);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            }

            var response = await ExecuteRequestAsync<Response<ProjectionCollection>>(request);

            return response.Data;
        }

        public async Task<DomainDto> GetDomainAsync(Connection connection)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, connection.CronusEndpoint + "/domain/explore");
            if (string.IsNullOrEmpty(connection.oAuth.ServerEndpoint) == false)
            {
                var accessToken = await token.GetAccessTokenAsync(connection);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            }

            var response = await ExecuteRequestAsync<DomainDto>(request);

            return response.Data;
        }

        public async Task<bool> RebuildAsync(Connection connection, Projection projection)
        {
            log.LogInformation("Rebuilding...");

            string resource = connection.CronusEndpoint + "/projection/rebuild";

            var rebuildRequest = new RebuildRequest()
            {
                ProjectionContractId = projection.ProjectionContractId,
                Hash = projection.LatestVersion.Hash
            };

            HttpRequestMessage request = CreateJsonPostRequest(rebuildRequest, resource);

            if (string.IsNullOrEmpty(connection.oAuth.ServerEndpoint) == false)
            {
                var accessToken = await token.GetAccessTokenAsync(connection);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            }

            await ExecuteRequestAsync<object>(request);

            return true;
        }

        public async Task<AggregateDto> GetAggregate(Connection connection, string aggregateId)
        {
            if (string.IsNullOrEmpty(aggregateId)) throw new ArgumentNullException(nameof(aggregateId));
            log.LogDebug($"GetAggregate({aggregateId})");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, connection.CronusEndpoint + $"/EventStore/Explore?id={aggregateId}");
            if (string.IsNullOrEmpty(connection.oAuth.ServerEndpoint) == false)
            {
                var accessToken = await token.GetAccessTokenAsync(connection);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            }

            var result = await ExecuteRequestAsync<Response<AggregateDto>>(request);

            return result.Data.Result;
        }

        public async Task<ProjectionStateDto> GetProjectionAsync(Connection connection, string projectionName, string projectionId)
        {
            if (string.IsNullOrEmpty(projectionName)) throw new ArgumentNullException(nameof(projectionName));
            if (string.IsNullOrEmpty(projectionId)) throw new ArgumentNullException(nameof(projectionId));

            log.LogDebug($"{projectionName}({projectionId})");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, connection.CronusEndpoint + $"/Projection/Explore?projectionName={projectionName}&id={projectionId}");
            if (string.IsNullOrEmpty(connection.oAuth.ServerEndpoint) == false)
            {
                var accessToken = await token.GetAccessTokenAsync(connection);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
            }

            var result = await ExecuteRequestAsync<Response<ProjectionStateDto>>(request);

            return result.Data.Result;
        }
    }

    public class DomainDto
    {
        public List<DomainAggregateDto> Aggregates { get; set; }

        public List<DomainGatewayDto> Gateways { get; set; }

        public List<DomainProjectionDto> Projections { get; set; }

        public List<DomainProjectionDto> Ports { get; set; }

        public List<DomainSagaDto> Sagas { get; set; }
    }

    public class DomainAggregateDto
    {
        public DomainAggregateDto()
        {
            Commands = new List<DomainCommandDto>();
            Events = new List<DomainEventDto>();
        }

        public string Name { get; set; }

        public List<DomainCommandDto> Commands { get; set; }
        public List<DomainEventDto> Events { get; set; }
    }

    public class DomainEventDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DomainPortDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DomainGatewayDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DomainSagaDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<DomainEventDto> Events { get; set; }
    }

    public class DomainCommandDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DomainProjectionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<DomainEventDto> Events { get; set; }
    }

    public class AggregateDto
    {
        public AggregateDto()
        {
            Commits = new List<AggregateCommitDto>();
        }

        public string BoundedContext { get; set; }

        public string AggregateId { get; set; }

        public List<AggregateCommitDto> Commits { get; set; }
    }

    public class ProjectionStateDto
    {
        public string Name { get; set; }
        public object State { get; set; }
    }

    public class AggregateCommitDto
    {
        public AggregateCommitDto()
        {
            Events = new List<EventDto>();
        }

        public int AggregateRootRevision { get; set; }

        public List<EventDto> Events { get; set; }

        public DateTime Timestamp { get; set; }
    }

    public class EventDto
    {
        public string EventName { get; set; }

        public object EventData { get; set; }
    }

    public class RebuildRequest
    {
        public string ProjectionContractId { get; set; }

        public string Hash { get; set; }
    }

    public class Response<T>
    {
        public T Result { get; set; }

        public string Errors { get; set; }

        public bool IsSuccess { get; set; }
    }
}
