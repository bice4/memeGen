import { useState, useEffect } from "react";
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { PickList } from 'primereact/picklist';

export default function CreateTemplate({ selectedPerson, selectedPhoto, onCallToast, onTemplateCreated }) {
    const [quotes, setQuotes] = useState([]);
    const [selectedBase64, setSelectedBase64] = useState('');
    const [target, setTarget] = useState([]);
    const [newTemplateName, setNewTemplateName] = useState('');

    const getQuotes = async () => {
        if (selectedPerson == null) return;
        fetch(`/api/Quote/person/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setQuotes(json);
            })
            .catch(error => {
                console.error('Error fetching quotes:', error);
                onCallToast(1, 'Failed to fetch quotes')
            });
    }

    const getPhotoContent = async (e) => {
        await fetch(`api/Photo/content/${e.id}`)
            .then((res) => res.text())
            .then(text => {
                setSelectedBase64(`data:image/png;base64,${text}`);
            })
            .catch(error => {
                console.error('Error fetching photo:', error);
                onCallToast(1, 'Failed to fetch photo')
            });
    }

    const addNewTemplate = async () => {
        const quoteNames = target.map(u => u.quote);
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
    }

    useEffect(() => {
        getQuotes();
        getPhotoContent(selectedPhoto);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson]);

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
        <Card title="Create template">
            <div className='flex flex-column'>
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

                        <div className='text-lg mt-4'>Title: {selectedPhoto.title}</div>
                        <div className='flex mt-4'>
                            {selectedBase64 && (
                                <img className='w-8 h-8' src={selectedBase64} />

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
            <div className='flex'>
                <Button className='ml-4' label='Create' icon='pi pi-upload' aria-label="Filter" onClick={addNewTemplate} disabled={isTemplateInvalid()} />
            </div>
        </Card>
    );
}