using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class graphForce : MonoBehaviour{

    private float dt;

    private bool isLeftFootOn = false;
    private bool isRightFootOn = false;
    private bool isLeftToeOn = false;
    private bool isRightToeOn = false;

    private float lastTime;

    public enum ExportType
    {
        rightFoot,
        rightToe,
        leftFoot,
        leftToe,
        chunk,
        Default
    }
    
    ExportType GetExportTypeFromGameObject(GameObject go)
    {
        switch (go.name)
        {
            case "mixamorig:LeftToeBase":
                return ExportType.leftToe;
            case "mixamorig:LeftFoot":
                return ExportType.leftFoot;
            case "mixamorig:RightToeBase":
                return ExportType.rightToe;
            case "mixamorig:RightFoot":
                return ExportType.rightFoot;
            case "chunk":
                return ExportType.chunk;
            default:
                return ExportType.Default;
        }
    }

    Dictionary<ExportType, string> paths = new Dictionary<ExportType, string>()
    {
        { ExportType.leftFoot, "/Exports/leftFoot.csv" },
        { ExportType.leftToe, "/Exports/leftToe.csv" },
        { ExportType.rightFoot, "/Exports/rightFoot.csv" },
        { ExportType.rightToe, "/Exports/rightToe.csv" },
        { ExportType.Default, "/Exports/default.csv" }
    };

    Dictionary<ExportType, bool> contactState = new Dictionary<ExportType, bool>()
    {
        { ExportType.leftFoot, false },
        { ExportType.leftToe, false },
        { ExportType.rightFoot, false },
        { ExportType.rightToe, false },
        { ExportType.Default, false }
    };

    string GetPath(ExportType type)
    {
        return Application.dataPath + paths[type];
    }

    void Start()
    { 
        if (!Directory.Exists(Application.dataPath + "/Exports"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Exports");
        }

        dt = Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        
        ExportType type = GetExportTypeFromGameObject(gameObject);
        string path = GetPath(type);
        if (contactState[type]) return; // déjà en contact

        contactState[type] = true;

        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine("On, , , ");
        }
    }
    void OnCollisionStay(Collision collision)
    {
        Rigidbody part = collision.rigidbody;
        ExportType type = GetExportTypeFromGameObject(gameObject);
        if (!contactState[type]) return;
        string path = GetPath(type);
        string lastLine = "";
        string firstArg = "";
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(Time.time.ToString("F4") + "," + gameObject.name + "," + collision.impulse.y / dt + "," + -GetComponent<Joint>().currentForce.y);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        ExportType type = GetExportTypeFromGameObject(gameObject);
        if (!contactState[type]) return;

        contactState[type] = false;
        string path = GetPath(type);
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine("Off, , , ");
        }
    }

}
