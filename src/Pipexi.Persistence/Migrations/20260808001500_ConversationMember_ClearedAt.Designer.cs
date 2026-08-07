using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pipexi.Persistence.Context;

#nullable disable

namespace Pipexi.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260808001500_ConversationMember_ClearedAt")]
partial class ConversationMember_ClearedAt
{
}
