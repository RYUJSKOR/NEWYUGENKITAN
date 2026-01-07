using UnityEngine;
using System.Collections.Generic;

public class TestController : MonoBehaviour
{
    [SerializeReference]
    public List<ITestStrategy> testList = new List<ITestStrategy>();
}