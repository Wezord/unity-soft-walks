using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HandStatWriter : MonoBehaviour
{
    public string filename;
    public Hand hand;

    private CSVWriter csv;
    private int frame = 0;

    void Start()
    {
		csv = new CSVWriter(filename);

		csv.Add("Frame");

        csv.AddVectorHeader("Linear Error");
		csv.AddVectorHeader("Target Position");
		csv.AddVectorHeader("Position");

		csv.AddVectorHeader("Target Velocity");
		csv.AddVectorHeader("Velocity");

		csv.Add("Linear spring");
		csv.Add("Linear damper");
		csv.Add("Linear force");
		csv.Add("Linear error velocity");

		csv.AddVectorHeader("Angular Error");
		csv.AddVectorHeader("Target Rotation");
		csv.AddVectorHeader("Rotation");

		csv.AddVectorHeader("Target Angular Velocity");
		csv.AddVectorHeader("Angular Velocity");

		csv.Add("Angular spring");
		csv.Add("Angular damper");
		csv.Add("Angular force");
		csv.Add("Angular error velocity");

		csv.NextLine();
	}

    void FixedUpdate()
    {
        if(!hand) return;

        csv.Add(frame.ToString());

        csv.Add(hand.GetLinearError());
		Vector3 target = hand.player.body.position + hand.position - hand.player.position;
		csv.Add(hand.player.transform.InverseTransformPoint(target));
		csv.Add(hand.transform.localPosition);

		csv.Add(hand.velocity);
		csv.Add(hand.body.velocity);

		csv.Add(hand.linearParameters.p1.ToString());
		csv.Add(hand.linearParameters.p2.ToString());
		csv.Add(hand.linearParameters.p3.ToString());
		csv.Add(hand.linearParameters.p4.ToString());


		csv.Add(hand.GetAngularError());
		csv.Add(hand.rotation.eulerAngles);
		csv.Add(hand.transform.rotation.eulerAngles);

		csv.Add(hand.angularVelocity);
		csv.Add(hand.body.angularVelocity);

		csv.Add(hand.angularParameters.p1.ToString());
		csv.Add(hand.angularParameters.p2.ToString());
		csv.Add(hand.angularParameters.p3.ToString());
		csv.Add(hand.angularParameters.p4.ToString());

		csv.NextLine();

		frame++;

    }

    void OnDestroy()
    {
		csv.Write();
	}   
}
