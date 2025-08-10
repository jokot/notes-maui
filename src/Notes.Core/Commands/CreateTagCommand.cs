using MediatR;
using Notes.Core.Models;

namespace Notes.Core.Commands;

public record CreateTagCommand(string Name, string Color = "#007ACC") : IRequest<Tag>;