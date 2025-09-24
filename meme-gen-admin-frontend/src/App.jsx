
import './App.css'
import { useState, useRef } from 'react';
import MainHeader from './components/MainHeader';
import Quotes from './components/Quotes';
import Photos from './components/Photos';
import CreateTemplate from './components/CreateTemplate';
import { Toast } from 'primereact/toast';
import Templates from './components/Templates';
import GeneratedGallery from './components/GeneratedGallery';
import { TabView, TabPanel } from 'primereact/tabview';
import Configuration from './components/Configuration';

export default function MyApp() {
  const [person, setPerson] = useState();
  const toast = useRef(null);
  const [createTemplateDialogVisible, setCreateTemplateDialogVisible] = useState(false);
  const [selectedPhotoForTemplate, setSelectedPhotoForTemplate] = useState();
  const [isConfigVisible, setIsConfigVisible] = useState(false);
  const [selectedTemplateForUpdate, setSelectedTemplateForUpdate] = useState();
  const [templateDialogMode, setTemplateDialogMode] = useState(-1);

  const handleOpenConfiguration = () => {
    setIsConfigVisible(!isConfigVisible);
  }

  const showCreateTemplateDialog = (e) => {
    setSelectedPhotoForTemplate(e);
    setCreateTemplateDialogVisible(true);
    setSelectedTemplateForUpdate();
    setTemplateDialogMode(0);
  }

  const showEditTemplate = (e) => {
    console.log(e);
    setSelectedTemplateForUpdate(e);
    setCreateTemplateDialogVisible(true);
    setTemplateDialogMode(1);
  }

  const hideCreateTemplateDialog = () => {
    setSelectedPhotoForTemplate();
    setCreateTemplateDialogVisible(false);
    setSelectedTemplateForUpdate();
    setTemplateDialogMode(-1);
  }

  const selectPerson = (e) => {
    setCreateTemplateDialogVisible(false);
    setIsConfigVisible(false);
    setSelectedPhotoForTemplate();
    setSelectedTemplateForUpdate();
    setPerson(e);
  }

  const showToast = (type, message) => {
    if (type === 1) {
      toast.current.show({
        severity: "error",
        summary: "Error",
        detail: message,
        life: 2000,
      });
    }

    if (type === 0) {
      toast.current.show({
        severity: "success",
        summary: "Success",
        detail: message,
        life: 2000,
      });
    }
  };

  return (
    <div>
      <MainHeader onSelectPerson={selectPerson} openConfiguration={handleOpenConfiguration} />
      {!createTemplateDialogVisible && !isConfigVisible && (
        <div>
          {person && (
            <TabView>
              <TabPanel header="Photos" leftIcon="pi pi-images mr-2">
                <div className='flex'>
                  <div className='col-12'>
                    <Photos selectedPerson={person} onCallToast={showToast} onCallCreateTemplate={showCreateTemplateDialog} />
                  </div>
                </div>
              </TabPanel>
              <TabPanel header="Quotes" leftIcon="pi pi-clipboard mr-2">
                <div className='flex'>
                  <div className='col-12'>
                    <Quotes selectedPerson={person} onCallToast={showToast} />
                  </div>
                </div>
              </TabPanel>
              <TabPanel header="Templates" leftIcon="pi pi-objects-column mr-2">
                <div className='flex'>
                  <div className='col-12'>
                    <Templates selectedPerson={person} onCallToast={showToast} onEditTemplate={showEditTemplate} />
                  </div>
                </div>
              </TabPanel>
              <TabPanel header="Gallery" leftIcon="pi pi-camera mr-2">
                <GeneratedGallery selectedPerson={person} onCallToast={showToast} />
              </TabPanel>
            </TabView>

          )}
        </div>
      )}
      {createTemplateDialogVisible && !isConfigVisible && (
        <CreateTemplate selectedPerson={person} selectedPhoto={selectedPhotoForTemplate} onCallToast={showToast} 
        onTemplateCreated={hideCreateTemplateDialog} selectedTemplate={selectedTemplateForUpdate}
        mode={templateDialogMode} />
      )}
      <Toast ref={toast} />
      {isConfigVisible && (
        <Configuration onCallToast={showToast} />
      )}
    </div>
  );
}
