using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pipexi.Persistence.Context;

#nullable disable

namespace Pipexi.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260801121500_ConversationMessage_ReactionsJson")]
partial class ConversationMessage_ReactionsJson
{
}
