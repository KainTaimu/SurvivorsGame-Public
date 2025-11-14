namespace SurvivorsGame.Pickups;

public partial class BaseXp : BasePickup
{
    public void SetXp(int xp)
    {
        var diff = xp * GD.RandRange(-0.5f, 0.5f);

        PickupValue = (int)Math.Ceiling(xp + diff);
    }
}

