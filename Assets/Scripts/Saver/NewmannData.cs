using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewmannData  {

	public double time;

	public Vector3 x_in; // vestibular position
	public Vector3 omega_in; // vestibular angular velocity

	public Vector3 xv_in; // visual position
	public Vector3 xvdot_in; // visual linear velocity
	public Vector3 omegav_in; // visual angular velocity

	public double sickness; //0..1

	public Vector3 theta; //vestibular orientation
	public Vector3 thetav; //visual orientation

	public NewmannData(double time, GameObject camera, Vector3 velocity, Vector3 omegaVest, Vector3 omegaVis, double sickness, Vector3 thetaVest, Vector3 thetaVis) {
		this.time = time;

		this.x_in = transformToNewmannCoordinate (camera.transform.localPosition);
		this.omega_in = -transformToNewmannCoordinate(omegaVest); // change from left to rigth hand coordinate turns

		// reverse these 3 variable (*-1) to make it work with Newmann
		this.xv_in = -transformToNewmannCoordinate(camera.transform.position);
		this.xvdot_in = -transformToNewmannCoordinate(velocity);
		this.omegav_in = transformToNewmannCoordinate(omegaVis); // change from left to rigth hand coordinate turns

		this.sickness = sickness;

		this.theta = -transformToNewmannCoordinate(thetaVest);
		this.thetav = -transformToNewmannCoordinate(thetaVis);
	}

	Vector3 transformToNewmannCoordinate(Vector3 unityVector) {
		return new Vector3 (unityVector.z, -unityVector.x, unityVector.y);
	}
}
