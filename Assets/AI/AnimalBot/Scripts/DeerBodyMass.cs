using UnityEngine;

[ExecuteAlways]
public class DeerBodyMass : MonoBehaviour{

    public bool hasRun = false;

    private float totalVolume = 0;
    private float sumPercentages = 0;

    private void Update(){
        if(hasRun) return;

		totalVolume = 0;
	    sumPercentages = 0;

	    BotParameters p = GetComponent<BotParameters>();
        var rootVolume = CalculateVolumes(transform);
        Debug.Log("Total volume: " + totalVolume);
        Debug.Log("Real Total volume: " + GetTotalVolume());
		p.bodyPercentage = rootVolume / totalVolume * 100;
		sumPercentages += p.bodyPercentage;
		CalculateBodyPercentages();
        Debug.Log("Sum bot joints volume percentage: " + sumPercentages);

        // add non-zero body percentage for the initial root
        if(p.bodyPercentage == 0){
            p.bodyPercentage = 1;
        }

        hasRun = true;
    }

    float GetTotalVolume(){
        float curVolume = 0;
        foreach(Collider collider in GetComponentsInChildren<Collider>()){
            float colliderVolume = GetVolume(collider);
            curVolume += colliderVolume;
        }
        return curVolume;
    }

    float CalculateVolumes(Transform bone)
    {
		float curVolume = 0;

		var collider = bone.GetComponent<Collider>();
		if (collider)
        {
			float colliderVolume = GetVolume(collider);

			totalVolume += colliderVolume;
			curVolume += colliderVolume;
			//Debug.Log("curVolume: " + curVolume + ", collider: " + collider.name);
		}

		foreach (Transform tf in bone)
		{
			if (tf.TryGetComponent<BotJoint>(out var joint))
			{
				joint.colliderVolume = CalculateVolumes(tf);
				continue;
			}
            curVolume += CalculateVolumes(tf);
		}

        return curVolume;
	}

    float GetVolume(Collider collider){
        float colliderVolume = 0;
        if(collider.GetType() == typeof(CapsuleCollider)){
            var colliderHeight = (collider as CapsuleCollider).height;
            var colliderDiameter = (collider as CapsuleCollider).radius * 2;
            colliderVolume = colliderDiameter * colliderDiameter * colliderHeight;
        } else if(collider.GetType() == typeof(BoxCollider)){
            var colliderSize = (collider as BoxCollider).size;
            colliderVolume = colliderSize.x * colliderSize.y * colliderSize.z;
        } else {
            Debug.Log("unknown collider type: " + collider);
        }
        return colliderVolume;        
    }

    void CalculateBodyPercentages(){
        foreach(var botJoint in GetComponentsInChildren<BotJoint>()){
            botJoint.bodyPercentage = botJoint.colliderVolume / totalVolume * 100;
            sumPercentages += botJoint.bodyPercentage;
        }
    }
}