using UnityEngine;

public class FactoryMachineInteraction : MonoBehaviour
{
    [SerializeField] private FactoryMachine machine;
    public FactoryMachine Machine => machine;

    private void Awake()
    {
        if (machine == null)
            machine = GetComponent<FactoryMachine>();
    }
}