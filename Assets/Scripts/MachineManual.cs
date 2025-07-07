using UnityEngine;

[CreateAssetMenu(fileName = "NewManual", menuName = "Maintenance/Manual")]
public class MachineManual : ScriptableObject
{
    public string machineName;
    public MaintenanceStep[] steps;
}
