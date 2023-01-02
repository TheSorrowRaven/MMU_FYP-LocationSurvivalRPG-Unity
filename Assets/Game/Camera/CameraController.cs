using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static G G => G.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;

    [SerializeField] private Transform PlayerTR;
    [SerializeField] private CinemachineBrain PlayerCinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera PlayerMapVCam;

    [System.NonSerialized] private CinemachineOrbitalTransposer PlayerMapOrbitalTransposer;

    [System.NonSerialized] private int lastFrameInput;
    [System.NonSerialized] private float dragInput;
    [System.NonSerialized] private Vector2 screenPosition;
    [System.NonSerialized] private Vector2 lastScreenPosition;

    private void Awake()
    {
        PlayerMapOrbitalTransposer = PlayerMapVCam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    private void Start()
    {
        G.ScreenInput.AddAsInputAction(ScreenDragInput);
    }

    //Unity Event Referenced
    public void ScreenDragInput(Vector2 screenPosition)
    {
        this.screenPosition = screenPosition;
        int currentFrame = Time.frameCount;
        if (currentFrame - lastFrameInput > 1)
        {
            lastScreenPosition = screenPosition;
        }
        lastFrameInput = currentFrame;
    }

    private void Update()
    {
        if (lastScreenPosition != screenPosition)
        {
            Vector2 playerCenter = G.MainCamera.WorldToScreenPoint(PlayerTR.localPosition);
            dragInput = Vector2.SignedAngle(lastScreenPosition - playerCenter, screenPosition - playerCenter);
            lastScreenPosition = screenPosition;
        }

        PlayerMapOrbitalTransposer.m_XAxis.Value += dragInput * GameSettings.PlayerMapOrbitalCameraSpeed;
        dragInput = Mathf.Lerp(dragInput, 0, 0.1f);
    }

}
