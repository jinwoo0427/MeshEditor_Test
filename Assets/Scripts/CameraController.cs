using GetampedPaint;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float _zoomSpeed = 0.5f;
    [SerializeField]
    float _zoomMax = -5f;
    [SerializeField]
    float _zoomMin = -15f;

    [SerializeField]
    float _RotateSpeed = -1f;
    [SerializeField]
    float _dragSpeed = 0.1f;
    [SerializeField]
    float _inputSpeed = 0.1f;

    public PaintManager texturePainter;

    private void LateUpdate()
    {
        CameraZoom();
        CameraDrag();
        CameraRotate();
        CameraInput();
    }

    void CameraRotate()
    {
        if (Input.GetMouseButton(1))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            Vector3 rotateValue = new Vector3(y, x * -1, 0);
            transform.eulerAngles = transform.eulerAngles - rotateValue;
            transform.eulerAngles += rotateValue * _RotateSpeed;
        }
    }

    void CameraZoom()
    {
        float _zoomDirection = Input.GetAxis("Mouse ScrollWheel");

        //Debug.Log(-_zoomDirection);

        // ���� �κ� ���߿� �Ÿ��� �����ؼ� �Ѱ��� ���ؾ� �ɵ�
        if (transform.position.z >= _zoomMax&& _zoomDirection > 0)
            return;

        if (transform.position.z <= _zoomMin && _zoomDirection < 0)
            return;
        transform.position += transform.forward * _zoomDirection * _zoomSpeed;
    }


    void CameraDrag()
    {
        if (Input.GetMouseButton(2))
        {
            float posX = Input.GetAxis("Mouse X");
            float posy = Input.GetAxis("Mouse Y");

            
            Quaternion v3Rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            // �÷��̾��� ��ġ���� ī�޶� �ٶ󺸴� ���⿡ ���Ͱ��� ������ ��� ��ǥ�� �����մϴ�.
            transform.position += v3Rotation * new Vector3(posX * -_dragSpeed,  posy * -_dragSpeed, 0f); 
        }
    }

    float totalRun = 1.0f;
    private void CameraInput()
    {
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.E))
            p_Velocity += new Vector3(0, 1f, 0);
        if (Input.GetKey(KeyCode.Q))
            p_Velocity += new Vector3(0, -1f, 0);
        if (Input.GetKey(KeyCode.W))
            p_Velocity += new Vector3(0, 0, 1f);
        if (Input.GetKey(KeyCode.S))
            p_Velocity += new Vector3(0, 0, -1f);
        if (Input.GetKey(KeyCode.A))
            p_Velocity += new Vector3(-1f, 0, 0);
        if (Input.GetKey(KeyCode.D))
            p_Velocity += new Vector3(1f, 0, 0);

        Vector3 p = p_Velocity;
        if (p.sqrMagnitude > 0)
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * 1.0f;

            p.x = Mathf.Clamp(p.x, -_inputSpeed, _inputSpeed);
            p.y = Mathf.Clamp(p.y, -_inputSpeed, _inputSpeed);
            p.z = Mathf.Clamp(p.z, -_inputSpeed, _inputSpeed);

            transform.Translate(p);
        }
    }
}