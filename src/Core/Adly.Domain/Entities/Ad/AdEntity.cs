using System.Diagnostics.CodeAnalysis;
using Adly.Domain.Common;
using Adly.Domain.Common.ValueObjects;
using Adly.Domain.Entities.User;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;

namespace Adly.Domain.Entities.Ad;

public sealed class AdEntity : BaseEntity<Guid>
{
    private readonly List<ImageValueObject> _images = new();
    private readonly List<LogValueObject> _changeLogs = new();
    public string Title { get; private set; }
    public string Description { get; private set; }


    public IReadOnlyList<ImageValueObject> Images => _images.AsReadOnly();
    public IReadOnlyList<LogValueObject> ChangeLogs => _changeLogs.AsReadOnly();

    public Guid UserId { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid LocationId { get; private set; }

    #region Navigation Properties

    public UserEntity User { get; private set; }
    public CategoryEntity Category { get; private set; }
    public LocationEntity Location { get; private set; }

    #endregion

    public AdStates CurrentState { get; private set; }


    public enum AdStates
    {
        Pending,
        Rejected,
        Approved,
        Deleted,
        Expired
    }

    public DomainResult ChangeState(AdStates state, string? additionalMessage = null)
    {
        if (CurrentState == AdStates.Approved
            && state is AdStates.Rejected or AdStates.Pending)
            return new DomainResult(false, "This ad is already approved!");

        CurrentState = state;

        this._changeLogs.Add(new LogValueObject(DateTime.Now, "Add State Changed", additionalMessage));


        return DomainResult.None;
    }


    private AdEntity()
    {
    }


    public static AdEntity Create(string title, string description, Guid? userId, Guid? categoryId, Guid? locationId)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        Guard.Against.NullOrEmpty(userId, message: "Invalid User ID");
        Guard.Against.NullOrEmpty(categoryId, message: "Invalid CategoryId");
        Guard.Against.NullOrEmpty(locationId, message: "Invalid location");

        var ad = new AdEntity()
        {
            Title = title,
            Id = Guid.NewGuid(),
            Description = description,
            CategoryId = categoryId.Value,
            UserId = userId.Value,
            CurrentState = AdStates.Pending,
            LocationId = locationId.Value
        };

        ad._changeLogs.Add(new LogValueObject(DateTime.Now, "Ad Created"));

        return ad;
    }
    
    
    public static AdEntity Create(string title, string description, UserEntity user, CategoryEntity category, LocationEntity location)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);

        Guard.Against.Null(user, message: "Invalid User ");
        Guard.Against.Null(category, message: "Invalid Category");
        Guard.Against.Null(location, message: "Invalid location");

        var ad = new AdEntity()
        {
            Title = title,
            Id = Guid.NewGuid(),
            Description = description,
            Category = category,
            CategoryId = category.Id,
            User = user,
            UserId = user.Id,
            CurrentState = AdStates.Pending,
            LocationId = location.Id,
            Location = location
        };

        ad._changeLogs.Add(new LogValueObject(DateTime.Now, "Ad Created"));

        return ad;
    }

    public static AdEntity Create(Guid? id, string title, string description, Guid? userId, Guid? categoryId,
        Guid? locationId)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);


        Guard.Against.NullOrEmpty(userId, message: "Invalid User ID");
        Guard.Against.NullOrEmpty(id, message: "Ivalid Id");
        Guard.Against.NullOrEmpty(categoryId, message: "invalid category ID");
        Guard.Against.NullOrEmpty(locationId, message: "Invalid location");


        var ad = new AdEntity()
        {
            Title = title,
            Id = id.Value,
            Description = description,
            CategoryId = categoryId.Value,
            UserId = userId.Value,
            CurrentState = AdStates.Pending,
            LocationId = locationId.Value
        };

        ad._changeLogs.Add(new LogValueObject(DateTime.Now, "Ad Created"));

        return ad;
    }


    public void Edit(string? title, string? description, Guid? categoryId, Guid? locationId)
    {

        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        if (categoryId.HasValue && categoryId != Guid.Empty)
            CategoryId = categoryId.Value;

        if (locationId.HasValue && locationId != Guid.Empty)
            LocationId = locationId.Value;
        
        _changeLogs.Add(new LogValueObject(DateTime.Now, "Ad Edited"));
        this.CurrentState = AdStates.Pending;
    }

    public void AddImage([NotNull] ImageValueObject image)
    {
        _images.Add(image);
    }

    public void RemoveImages(string[] imageNames)
    {
        _images.RemoveAll(c => imageNames.Contains(c.FileName));
    }
}