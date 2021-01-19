﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

namespace CAS
{
    public class CAS_EachStepManager : MonoBehaviour
    {
        CAS_StepManager stepManager; 

        //Model Information 
        Dictionary<string, GameObject> modelsInThisStep;

        CAS_PlaceModels placeModels;

        [HideInInspector]
        public bool initial = false; 

        int a = 0;

        Color[] colorsForGroupBy = new Color[]{new Color(0.5f, 0f, 0f, 0f), new Color(0f, 0.5f, 0f, 0f), new Color(0f, 0f, 0.5f, 0f) , new Color(0.5f, 0.5f, 0f, 0f) , new Color(0.5f, 0f, 0.5f, 0f), new Color(0f, 0.5f, 0.5f, 0f)};

        public int stepIndex; 
        private void Awake()
        {
            stepManager = GetComponentInParent<CAS_StepManager>();
            placeModels = GetComponent<CAS_PlaceModels>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public Dictionary<string, GameObject> GetModelsInThisStep()
        {
            return modelsInThisStep; 
        }

        public void PlaceModels() 
        {        
            if(!placeModels) placeModels = GetComponent<CAS_PlaceModels>();

            placeModels.numberOfModelsAtFirstCircle = 24;
            placeModels.adjustmentDistance = 0.002f;
            placeModels.distanceBetweenSmallCircles = 0.0725f;
            placeModels.adjustmentAngle = 0f; 
            placeModels.radiusOfTheSphere = 0.75f;
            placeModels.PickAndPlace();
        }

        public void GroupModels(Dictionary<string, List<string>> filteredPatientIdsGroupBy)
        {
            int index = 0;

            foreach (string key in filteredPatientIdsGroupBy.Keys.ToList())
            {
                foreach (string value in filteredPatientIdsGroupBy[key])
                {
                    if (stepManager.allModelsInformation.ContainsKey(value))
                    {
                        //stepManager.allModelsInformation[value].GetComponentInChildren<MeshRenderer>().material.color = colorsForGroupBy[index];
                        stepManager.allModelsInformation[value].GetComponentInChildren<CAS_ContolModel>().SetDefaultColor(colorsForGroupBy[index]);
                    }
                }
                index++;
            }
        }

        public void SetModels(List<GameObject> models)
        {
            modelsInThisStep = new Dictionary<string, GameObject>();
            foreach (GameObject model in models)
            {
                modelsInThisStep.Add(model.name, model);
                model.transform.parent = transform;
            }
        }

        public void SetModelsLayer(List<GameObject> models)
        {
            SetModels(models); 
        }

        public void SetModelsInitial(List<GameObject> models)
        {
            SetModels(models);
            PlaceModels();
        }

        public void MoveModelToLayerPosition()
        {
            foreach(Transform child in transform)
            {
                child.GetComponent<CAS_ContolModel>().ChangeLayer(); 
            }
        }

        Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t)
        {
            return A + (B - A) * t;
        }
    }
}