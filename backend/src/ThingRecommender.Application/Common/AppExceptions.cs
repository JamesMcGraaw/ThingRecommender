namespace ThingRecommender.Application.Common;

public class NotFoundException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);
