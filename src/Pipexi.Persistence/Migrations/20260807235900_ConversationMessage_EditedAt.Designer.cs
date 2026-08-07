using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pipexi.Persistence.Context;

#nullable disable

namespace Pipexi.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260807235900_ConversationMessage_EditedAt")]
partial class ConversationMessage_EditedAt
{
}
