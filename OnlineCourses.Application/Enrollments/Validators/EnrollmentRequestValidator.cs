using FluentValidation;
using OnlineCourses.Application.Enrollments.DTOs;

namespace OnlineCourses.Application.Enrollments.Validators;

public class EnrollmentRequestValidator : AbstractValidator<EnrollmentRequest>
{
    public EnrollmentRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");
    }
}