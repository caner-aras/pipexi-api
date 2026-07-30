using Pipexi.Application.Abstractions.Identity;

namespace Pipexi.Application.Common.Authorization;

public static class OrganizationIdResolver
{
    public static Guid? Resolve(object request, ICurrentUserContext currentUserContext)
    {
        var requestType = request.GetType();

        var scopedOrganizationId = ReadGuidProperty(requestType, request, "ScopedOrganizationId");
        if (scopedOrganizationId.HasValue && scopedOrganizationId.Value != Guid.Empty)
        {
            return scopedOrganizationId.Value;
        }

        var organizationId = ReadOrganizationIdProperty(requestType, request);
        if (organizationId.HasValue && organizationId.Value != Guid.Empty)
        {
            return organizationId.Value;
        }

        if (currentUserContext.OrganizationId != Guid.Empty)
        {
            return currentUserContext.OrganizationId;
        }

        return null;
    }

    public static bool RequiresOrganizationContext(object request)
    {
        var requestType = request.GetType();

        if (requestType.GetProperty("ScopedOrganizationId") is not null)
        {
            return true;
        }

        if (requestType.GetProperty("OrganizationId") is not null)
        {
            return true;
        }

        return false;
    }

    private static Guid? ReadOrganizationIdProperty(Type requestType, object request)
    {
        var property = requestType.GetProperty("OrganizationId");
        if (property is null)
        {
            return null;
        }

        if (property.PropertyType == typeof(Guid))
        {
            return (Guid)property.GetValue(request)!;
        }

        if (property.PropertyType == typeof(Guid?))
        {
            return (Guid?)property.GetValue(request);
        }

        return null;
    }

    private static Guid? ReadGuidProperty(Type requestType, object request, string propertyName)
    {
        var property = requestType.GetProperty(propertyName);
        if (property is null)
        {
            return null;
        }

        if (property.PropertyType == typeof(Guid))
        {
            return (Guid)property.GetValue(request)!;
        }

        if (property.PropertyType == typeof(Guid?))
        {
            return (Guid?)property.GetValue(request);
        }

        return null;
    }
}
