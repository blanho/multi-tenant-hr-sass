using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Employee.Domain.ValueObjects;

public sealed record Department
{
    public string Name { get; }

    private Department(string name) => Name = name;

    public static Department Create(string name)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.MaxLength(name.Trim(), 100, nameof(name));
        return new Department(name.Trim());
    }

    public override string ToString() => Name;
}

public sealed record Position
{
    public string Title { get; }

    private Position(string title) => Title = title;

    public static Position Create(string title)
    {
        Guard.NotNullOrWhiteSpace(title, nameof(title));
        Guard.MaxLength(title.Trim(), 100, nameof(title));
        return new Position(title.Trim());
    }

    public override string ToString() => Title;
}
