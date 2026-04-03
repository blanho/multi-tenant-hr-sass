using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Employee.Domain.ValueObjects;

public record Department
{
    public string Name { get; }

    private Department(string name) => Name = name;

    public static Department Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name is required.");

        if (name.Length > 100)
            throw new DomainException("Department name cannot exceed 100 characters.");

        return new Department(name.Trim());
    }

    public override string ToString() => Name;
}

public record Position
{
    public string Title { get; }

    private Position(string title) => Title = title;

    public static Position Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Position title is required.");

        if (title.Length > 100)
            throw new DomainException("Position title cannot exceed 100 characters.");

        return new Position(title.Trim());
    }

    public override string ToString() => Title;
}
