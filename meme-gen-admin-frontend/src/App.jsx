
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

export default function MyApp() {
  const [person, setPerson] = useState();
  const toast = useRef(null);
  const [createTemplateDialogVisible, setCreateTemplateDialogVisible] = useState(false);
  const [selectedPhotoForTemplate, setSelectedPhotoForTemplate] = useState();
  const [quotesCount, setQuotesCount] = useState(0);

  const showCreateTemplateDialog = (e) => {
    if (quotesCount === 0) {
      showToast(1, "No quotes found");
      return;
    }

    setSelectedPhotoForTemplate(e);
    setCreateTemplateDialogVisible(true);
  }

  const hideCreateTemplateDialog = () => {
    setSelectedPhotoForTemplate();
    setCreateTemplateDialogVisible(false);
  }

  const selectPerson = (e) => {
    setCreateTemplateDialogVisible(false);
    setSelectedPhotoForTemplate();
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
      <MainHeader onSelectPerson={selectPerson} />
      {!createTemplateDialogVisible && (
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
                    <Quotes selectedPerson={person} onCallToast={showToast} onCallQuotesCount={setQuotesCount} />
                  </div>
                </div>
              </TabPanel>
              <TabPanel header="Templates" leftIcon="pi pi-objects-column mr-2">
                <div className='flex'>
                  <div className='col-12'>
                    <Templates selectedPerson={person} onCallToast={showToast} />
                  </div>
                </div>
              </TabPanel>
              <TabPanel header="Gallery" leftIcon="pi pi-camera mr-2">
                <GeneratedGallery onCallToast={showToast} />
              </TabPanel>
            </TabView>

          )}
        </div>
      )}
      {createTemplateDialogVisible && (
        <CreateTemplate selectedPerson={person} selectedPhoto={selectedPhotoForTemplate} onCallToast={showToast} onTemplateCreated={hideCreateTemplateDialog} />
      )}
      <Toast ref={toast} />
    </div>
  );
}
