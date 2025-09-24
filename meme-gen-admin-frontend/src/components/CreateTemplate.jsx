import { useState, useEffect } from "react";
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { PickList } from 'primereact/picklist';
import { Divider } from "primereact/divider";

export default function CreateTemplate({ selectedPerson, selectedPhoto, onCallToast, onTemplateCreated, selectedTemplate, mode }) {
    const [quotes, setQuotes] = useState([]);
    const [imageBase64, setImageBase64] = useState('');
    const [target, setTarget] = useState([]);
    const [newTemplateName, setNewTemplateName] = useState('');
    const [photoTitle, setPhotoTitle] = useState('');

    const getCreationInfo = async () => {
        if (selectedPerson === null) return;

        if (mode == 1) return;

        fetch(`/api/Template/createInfo/${selectedPhoto.id}/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setQuotes(json.quotes);
                setImageBase64(`data:image/png;base64,${json.photoBase64}`);
                setPhotoTitle(json.photoTitle);
            })
            .catch(error => {
                console.error('Error fetching quotes:', error);
                onCallToast(1, 'Failed to fetch creation information')
            });
    }

    const getUpdateInfo = async () => {

        if (mode == 0) return;

        await fetch(`api/Template/updateInfo/${selectedTemplate.id}`)
            .then((res) => res.json())
            .then(json => {
                setImageBase64(`data:image/png;base64,${json.photoBase64}`);
                setPhotoTitle(json.photoTitle);
                setQuotes(json.quotesToAdd);
                setTarget(json.templateQuotes);
                setNewTemplateName(json.name);
            })
            .catch(error => {
                console.error('Error fetching photo:', error);
                onCallToast(1, 'Failed to fetch update information')
            });
    }

    const addorUpdateTemplate = async () => {
        const quoteNames = target.map(u => u.quote);

        if (mode == 0) {
            const response = await fetch('api/template', {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ Name: newTemplateName, PersonId: selectedPerson.id, PhotoId: selectedPhoto.id, Quotes: quoteNames, PhotoTitle: selectedPhoto.title, PersonName: selectedPerson.name })
            });

            if (response.ok) {
                onCallToast(0, 'New template added');
                onTemplateCreated();
            } else {
                console.error("Error:", response.status);
                onCallToast(1, 'Failed to add template');
            }
        } else {
            const response = await fetch('api/template', {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ Name: newTemplateName, Id: selectedTemplate.id, Quotes: quoteNames })
            });

            if (response.ok) {
                onCallToast(0, 'Template updated');
                onTemplateCreated();
            } else {
                console.error("Error:", response.status);
                onCallToast(1, 'Failed to update template');
            }
        }
    }

    useEffect(() => {

        getUpdateInfo(selectedPhoto);
        getCreationInfo();

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson, selectedTemplate, mode]);

    const onChange = (event) => {
        setQuotes(event.source);
        setTarget(event.target);
    };

    const itemTemplate = (item) => {
        return (
            <div className="flex flex-wrap align-items-center gap-3">
                <div className="flex-1 flex flex-column gap-2">
                    <span className="font-bold">{item.quote}</span>
                </div>
            </div>
        );
    };

    function isTemplateInvalid() {
        if (newTemplateName === '' || newTemplateName === undefined) return true;
        if (target.length === 0) return true;
        return false;
    }

    return (
        <div className="ml-5">
            <Divider />
            <div className='text-2xl'>
                {mode === 0 ? 'Create' : 'Update'}  template
            </div>
            <div className='flex flex-column mt-5'>
                <div className='flex flex-column'>
                    <div className='text-lg'>Name</div>
                    <div className='flex mt-2'>
                        <InputText className='w-2' value={newTemplateName} onChange={(e) => setNewTemplateName(e.target.value)} />
                    </div>
                </div>
            </div>
            <div className='flex mt-5'>
                <div className='col-3'>
                    <div className='flex flex-column'>
                        <div className='text-xl'>Selected photo</div>

                        <div className='text-lg mt-4'>Title: {photoTitle}</div>
                        <div className='flex mt-4'>
                            {imageBase64 && (
                                <img className='w-8 h-8' src={imageBase64} style={{
                                        borderRadius: "12px",
                                        boxShadow: "0 8px 20px rgba(0,0,0,0.3)",
                                    }} />
                            )}
                        </div>
                    </div>
                </div>
                <div className='col-6'>
                    <div className='flex flex-column'>
                        <div className='text-xl'>Select quotes</div>
                        <PickList className='mt-4' dataKey="id" source={quotes} target={target} itemTemplate={itemTemplate} onChange={onChange} breakpoint="1280px"
                            sourceHeader="Available" targetHeader="Selected" sourceStyle={{ height: '24rem' }} targetStyle={{ height: '24rem' }} />
                    </div>
                </div>

            </div>
            <div className='flex flex-column'>
                <Divider />

                <Button className='ml-4 w-1' label={mode == 0 ? 'Create' : 'Update'} icon='pi pi-upload' aria-label="Filter" onClick={addorUpdateTemplate} disabled={isTemplateInvalid()} />
            </div>
        </div>
    );
}