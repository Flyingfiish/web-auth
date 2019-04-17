﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace PhotoApp.Services.TicketStores
{
    public class EntityTicketStore : ITicketStore
    {
        readonly TicketsDbContext dbContext;

        public EntityTicketStore(TicketsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task RemoveAsync(string key)
        {
            if (Guid.TryParse(key, out var id))
            {
                var ticketEntity = await dbContext.Tickets.SingleOrDefaultAsync(x => x.Id == id);
                if (ticketEntity != null)
                {
                    dbContext.Tickets.Remove(ticketEntity);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (Guid.TryParse(key, out var id))
            {
                var ticketEntity = await dbContext.Tickets.FindAsync(id);
                if (ticketEntity != null)
                {
                    ticketEntity.Value = SerializeToBytes(ticket);
                    ticketEntity.LastActivity = DateTimeOffset.UtcNow;
                    ticketEntity.Expires = ticket.Properties.ExpiresUtc;
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            if (Guid.TryParse(key, out var id))
            {
                var ticketEntity = await dbContext.Tickets.FindAsync(id);
                if (ticketEntity != null)
                {
                    ticketEntity.LastActivity = DateTimeOffset.UtcNow;
                    await dbContext.SaveChangesAsync();

                    return DeserializeFromBytes(ticketEntity.Value);
                }
            }

            return null;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var userId = Guid.Parse(ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
            var authenticationTicket = new TicketEntity
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket),
            };

            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                authenticationTicket.Expires = expiresUtc.Value;
            }

            await dbContext.Tickets.AddAsync(authenticationTicket);
            await dbContext.SaveChangesAsync();

            return authenticationTicket.Id.ToString();
        }

        private byte[] SerializeToBytes(AuthenticationTicket source)
            => TicketSerializer.Default.Serialize(source);

        private AuthenticationTicket DeserializeFromBytes(byte[] source)
            => source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}