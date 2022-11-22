using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameSettings GameSettings => GameSettings.Instance;

    [SerializeField] private CinemachineBrain PlayerCinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera PlayerMapVCam;

    [System.NonSerialized] private CinemachineOrbitalTransposer PlayerMapOrbitalTransposer;

    [System.NonSerialized] private Vector2 dragInput;

    private void Awake()
    {
        PlayerMapOrbitalTransposer = PlayerMapVCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    //Unity Event Referenced
    public void ScreenDragInput(Vector2 drag)
    {
        dragInput = drag;
    }

    private void Update()
    {
        PlayerMapOrbitalTransposer.m_XAxis.Value += dragInput.x * GameSettings.PlayerMapOrbitalCameraSpeed;
        dragInput = Vector2.Lerp(dragInput, Vector2.zero, 0.1f);
    }

}
