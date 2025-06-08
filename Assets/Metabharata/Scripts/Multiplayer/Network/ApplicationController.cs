using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    async void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
