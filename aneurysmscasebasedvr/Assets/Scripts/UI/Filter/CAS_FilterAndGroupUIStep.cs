﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CAS
{
    public class CAS_FilterAndGroupUIStep : MonoBehaviour
    {
        public CAS_FilterAndGroupUIManager filterAndGroupUIManager; 

        CAS_TabGroup tabGroup;
        public CAS_TabButton[] tabButtons;

        public GameObject displayListPrefab;

        [HideInInspector]
        public List<CAS_FilterAndGroupOptionKeyValuesClass> filtersApplied;

        [HideInInspector]
        public List<CAS_EachFilterAndGroupStep> eachFilterAndGroupStepsAdded;

        [HideInInspector]
        public int currentStep = -1;

        public GameObject filterLayerPagePrefab;
        public GameObject filterLayerButtonPrefab;
        public GameObject filterLayerPageParent;
        public GameObject filterLayerButtonParent;
        public List<GameObject> filterLayerPageList;
        public List<GameObject> filterLayerButtonList;

        private void Awake()
        {
            filtersApplied = new List<CAS_FilterAndGroupOptionKeyValuesClass>();
            eachFilterAndGroupStepsAdded = new List<CAS_EachFilterAndGroupStep>();

            filterLayerPageList = new List<GameObject>();
            filterLayerButtonList = new List<GameObject>();

            filterAndGroupUIManager = GetComponentInParent<CAS_FilterAndGroupUIManager>(); 
        }

        // Start is called before the first frame update
        void Start()
        {
            //tabButtons = GetComponentsInChildren<CAS_TabButton>();
            tabGroup = GetComponentInChildren<CAS_TabGroup>();         
        }

        // Update is called once per frame
        void Update()
        {
       
        }

        public int GetCurrentStep()
        {
            return currentStep; 
        }

        public void AddFilter(string filterKey, List<string> filterValuesString,  List<double> filterValuesDouble, bool isString)
        {
            if(filterValuesString.Count > 0 || filterValuesDouble.Count > 0){
                CAS_FilterAndGroupOptionKeyValuesClass filterKeyValuesClass = new CAS_FilterAndGroupOptionKeyValuesClass(filterKey, filterValuesString, filterValuesDouble, isString);
                filtersApplied.Add(filterKeyValuesClass);
            }

            ApplyFilter(filtersApplied); 
        }

        public void ChangeFilter(string filterKey, List<string> filterValuesString, List<double> filterValuesDouble, bool isString)
        {
            //foreach (CAS_FilterKeyValuesClass eachKeyFilterValues in filtersApplied)
            for(int i = 0; i < filtersApplied.Count; i++)
            {
                CAS_FilterAndGroupOptionKeyValuesClass eachKeyFilterValues = filtersApplied[i];
                if (eachKeyFilterValues.GetFilterName() == filterKey)
                {
                    if (isString)   
                    {
                        if (filterValuesString.Count > 0)
                        {
                            eachKeyFilterValues.SetStringValues(filterValuesString);
                        }
                        else
                        {
                            filtersApplied.Remove(eachKeyFilterValues);
                        }
                    }
                    else
                    {
                        if(filterValuesDouble.Count > 0)
                        {
                            eachKeyFilterValues.SetDoubleValues(filterValuesDouble);
                        }
                        else
                        {
                            filtersApplied.Remove(eachKeyFilterValues);
                        }
                    }
                }
            }

            ApplyFilter(filtersApplied); 
        }

        public CAS_EachFilterAndGroupStep CreateFilterLayer()
        {
            GameObject filterLayerButton = Instantiate(filterLayerButtonPrefab, filterLayerButtonParent.transform);
            filterLayerButton.name = "Filter Layer Button" + (filterLayerButtonList.Count + 1);
            filterLayerButton.GetComponent<CAS_TabButton>().tabGroup = tabGroup;
            filterLayerButton.GetComponent<CAS_TabButton>().SetButtonText("" + (filterLayerButtonList.Count + 1));
            filterLayerButtonList.Add(filterLayerButton);

            GameObject filterLayerPage = Instantiate(filterLayerPagePrefab, filterLayerPageParent.transform);
            filterLayerPage.name = "Filter Layer Page" + (filterLayerPageList.Count + 1); 
            filterLayerPageList.Add(filterLayerPage);
            tabGroup.SetObjectsToSwap(filterLayerPageList);

            CAS_EachFilterAndGroupStep eachFilterAndGroupStep = filterLayerPage.GetComponent<CAS_EachFilterAndGroupStep>();
            
            return eachFilterAndGroupStep; 
        }

        public void DeleteFilterLayer(int index)
        {
            eachFilterAndGroupStepsAdded.RemoveAt(index); 

            Destroy(filterLayerPageList[index].gameObject); 
            filterLayerPageList.RemoveAt(index);
            tabGroup.SetObjectsToSwap(filterLayerPageList);

            tabGroup.UnSubcribe(filterLayerButtonList[index].gameObject.GetComponent<CAS_TabButton>()); 
            Destroy(filterLayerButtonList[index].gameObject);
            filterLayerButtonList.RemoveAt(index);
        }

        public void OpenClose(bool status)
        {
            if (status)
            {
                GetComponent<CanvasGroup>().alpha = 1;
            }
            else
            {
                GetComponent<CanvasGroup>().alpha = 0; 
            }

            GetComponent<CanvasGroup>().interactable = status;
            GetComponent<TrackedDeviceGraphicRaycaster>().enabled = status;
        }

        public void ApplyFilter(List<CAS_FilterAndGroupOptionKeyValuesClass> filterKeyValues)
        {
            List<CAS_FilterAndGroupOptionKeyValuesClass> incrementalStepFilterKeyValuesList = new List<CAS_FilterAndGroupOptionKeyValuesClass>();
            List<List<string>> modeldsForThisStep = new List<List<string>>();

            foreach (CAS_FilterAndGroupOptionKeyValuesClass eachStepFilterKeyValues in filterKeyValues)
            {
                incrementalStepFilterKeyValuesList.Add(eachStepFilterKeyValues);
                modeldsForThisStep.Add(GetFilteredPatiendIds(incrementalStepFilterKeyValuesList));
            }

            filterAndGroupUIManager.manager.stepManager.SetFilteredModelsToEditLayers(modeldsForThisStep);

            CreateAndEditFilterSteps(); 
        }


        public void CreateAndEditFilterSteps()
        {
            int index = -1;

            List<CAS_FilterAndGroupOptionKeyValuesClass> filtersAppliedAtEachStep = new List<CAS_FilterAndGroupOptionKeyValuesClass>(); 
            foreach (CAS_FilterAndGroupOptionKeyValuesClass filterKeyValuesClass in filtersApplied)
            {
                index++; 
                filtersAppliedAtEachStep.Add(filterKeyValuesClass);

                if (index >= eachFilterAndGroupStepsAdded.Count)
                {
                    CAS_EachFilterAndGroupStep eachFilterAndGroupStep = CreateFilterLayer(); 
                    eachFilterAndGroupStepsAdded.Add(eachFilterAndGroupStep);
                    eachFilterAndGroupStep.SetDisplayContent(filtersAppliedAtEachStep);
                }
                else
                {
                    eachFilterAndGroupStepsAdded[index].SetDisplayContent(filtersAppliedAtEachStep);
                }
            }

            //Remove unnecessary steps 
            if (eachFilterAndGroupStepsAdded.Count - index > 1)
            {
                int stepsToDelete = eachFilterAndGroupStepsAdded.Count - index - 1;

                int lastStep = eachFilterAndGroupStepsAdded.Count - 1;


                for (int i = lastStep; i > lastStep - stepsToDelete; i--)
                {
                    DeleteFilterLayer(i);
                }
            }
        }

        public List<string> GetFilteredPatiendIds(List<CAS_FilterAndGroupOptionKeyValuesClass> filterKeyValues)
        {
            List<string> stringFilterKeys = new List<string>();
            List<List<string>> stringFilterValues = new List<List<string>>();
            List<string> doubleFilterKeys = new List<string>();
            List<List<double>> doubleFilterValues = new List<List<double>>();

            foreach (CAS_FilterAndGroupOptionKeyValuesClass filterKeyValue in filterKeyValues)
            {
                if (filterKeyValue.GetIsString())
                {
                    stringFilterKeys.Add(filterKeyValue.GetFilterName());
                    stringFilterValues.Add(filterKeyValue.GetStringValues());
                }
                else
                {
                    doubleFilterKeys.Add(filterKeyValue.GetFilterName());
                    doubleFilterValues.Add(filterKeyValue.GetDoubleValues());
                }
            }

            Debug.Log(stringFilterKeys.Count);
            Debug.Log(doubleFilterKeys.Count);

            return filterAndGroupUIManager.manager.dataManager.GetFilteredPatientIdsStringAndInteger(stringFilterKeys, stringFilterValues, doubleFilterKeys, doubleFilterValues);
        }
    }
}
