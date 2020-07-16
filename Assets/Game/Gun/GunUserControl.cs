using UnityEngine;

public class GunUserControl : MonoBehaviour
{
    private float _sensitivity;
    public Transform Gun;
    private Transform _gunBoneDown;
    private Transform _gunBoneCenter;
    private Transform _gunBoneUp;
    private Quaternion _gunBoneDownTargetRotation;
    private Quaternion _gunBoneCenterTargetRotation;
    private Quaternion _gunBoneUpTargetRotation;
    private bool _cursorLock;

    void Start()
    {
        _cursorLock = true;
        _gunBoneDown = Gun.Find("Armature/Gun1/Gun2");
        _gunBoneCenter = _gunBoneDown.Find("Gun3");
        _gunBoneUp = _gunBoneCenter.Find("Gun4");
        _gunBoneDownTargetRotation = _gunBoneDown.localRotation;
        _gunBoneCenterTargetRotation = _gunBoneCenter.localRotation;
        _gunBoneUpTargetRotation = _gunBoneUp.localRotation;
        UpdateSensitivity();
    }

    private void UpdateSensitivity()
    {
        if (PlayerPrefs.HasKey("mouseSensitivity"))
        {
            _sensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
        }
        else
        {
            _sensitivity = 2;
        }
    }

    private void OnEnable()
    {
        PauseMenuController.AddOnShowHideAction(OnShowHideMenu);
    }

    private void OnDisable()
    {
        PauseMenuController.RemoveOnShowHideAction(OnShowHideMenu);
    }

    private void OnShowHideMenu(bool show)
    {
        _cursorLock = !show;
        UpdateSensitivity();
    }

    void Update()
    {
        if (!_cursorLock)
        {
            InternalLockUpdate();
            return;
        }

        var horizontalAxis = Input.GetAxis("Mouse X");
        var verticalAxis = Input.GetAxis("Mouse Y");

        _gunBoneCenterTargetRotation *=
            Quaternion.Euler(0, horizontalAxis * _sensitivity * Time.deltaTime, 0);
        _gunBoneCenterTargetRotation = ClampRotationForCenterBone(_gunBoneCenterTargetRotation);
        _gunBoneCenter.localRotation = _gunBoneCenterTargetRotation;

        _gunBoneDownTargetRotation *= Quaternion.Euler(-verticalAxis * _sensitivity * Time.deltaTime, 0, 0);
        _gunBoneDownTargetRotation = ClampRotationForDownBone(_gunBoneDownTargetRotation);
        _gunBoneDown.localRotation = _gunBoneDownTargetRotation;

        _gunBoneUpTargetRotation *= Quaternion.Euler(-verticalAxis * _sensitivity * Time.deltaTime, 0, 0);
        _gunBoneUpTargetRotation = ClampRotationForUpBone(_gunBoneUpTargetRotation);
        _gunBoneUp.localRotation = _gunBoneUpTargetRotation;

        InternalLockUpdate();
    }

    private Quaternion ClampRotationForCenterBone(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        angleY = Mathf.Clamp(angleY, -60f, 60f);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
        q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY / 2.27f);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * 44);
        return q;
    }

    private Quaternion ClampRotationForDownBone(Quaternion q)
    {
        int limit = 15;
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, 37 - limit, 37 + limit);
        q.y = 0;
        q.z = 0;
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }


    private Quaternion ClampRotationForUpBone(Quaternion q)
    {
        int limit = 15;
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, 44 - limit, 44 + limit);
        q.y = 0;
        q.z = 0;
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }

    private void InternalLockUpdate()
    {
        if (_cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!_cursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}