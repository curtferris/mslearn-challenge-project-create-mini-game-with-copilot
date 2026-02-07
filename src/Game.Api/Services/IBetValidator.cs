using Game.Shared.Models;

namespace Game.Api.Services;

public interface IBetValidator
{
    void Validate(PlayerState currentState, PlayRequest request);
}
