using ShuffleDataMasking.Domain.Masking.Messages;
using FluentValidation;

namespace ShuffleDataMasking.Domain.Masking.Validations
{
    public class ShuffleDataMaskingMessageValidator : AbstractValidator<ShuffleDataMaskingMessage>
    {
        public ShuffleDataMaskingMessageValidator()
        {
            RuleFor(s => s.Database)
                .NotEmpty()
                .NotNull()
                .WithMessage(m => $"Database does not exist.");

            RuleFor(s => s.TableQuery)
                .NotEmpty()
                .NotNull()
                .WithMessage(m => $"TableQuery does not exist.");

            RuleFor(s => s.TableQueryId)
                .NotNull()
                .WithMessage(m => $"TableQueryId is invalid.");

            RuleFor(s => s.StartQuery)
                .NotNull()
                .GreaterThan(-1)
                .WithMessage(m => $"StartQuery is invalid.");

            RuleFor(s => s.EndQuery)
                .NotNull()
                .GreaterThan(-1)
                .WithMessage(m => $"EndQuery is invalid.");

            RuleFor(s => s.QueryProcessId)
                .NotNull()
                .GreaterThan(-1)
                .WithMessage(m => $"QueryProcessId is invalid.");
        }
    }
}
