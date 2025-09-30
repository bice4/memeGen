import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect, useRef } from "react";
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { ContextMenu } from 'primereact/contextmenu';
import { Skeleton } from 'primereact/skeleton';
import { FileUpload } from 'primereact/fileupload';
import { Tag } from 'primereact/tag';

export default function Photos({ selectedPerson, onCallToast, onCallCreateTemplate }) {
    const [isLoading, setIsLoading] = useState(true);
    const [photos, setPhotos] = useState();
    const [newPhotoTitle, setNewPhotoTitle] = useState('');
    const [selectedPhoto, setSelectedPhoto] = useState();
    const cm = useRef(null);
    const [newPhotoBase64, setNewPhotoBase64] = useState('');
    const [selectedBase64, setSelectedBase64] = useState('');

    const apiUrl = '/api/Photo';

    const menuModel = [
        { label: 'Delete', icon: 'pi pi-fw pi-times', command: () => deletePhoto(selectedPhoto) },
        {
            label: 'Create template', icon: 'pi pi-fw pi-file-plus', command: () => {
                onCallCreateTemplate(selectedPhoto);
            }
        }
    ];

    const deletePhoto = async () => {
        const response = await fetch(`${apiUrl}/${selectedPhoto.id}`, {
            method: "DELETE",
        });

        setSelectedBase64('');

        if (response.ok) {
            onCallToast(0, 'Photo deleted');
            getPhotosByPersonId();

        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to delete photo');
        }
    };

    const getPhotosByPersonId = async () => {
        if (selectedPerson == null) return;
        setIsLoading(true);
        fetch(`${apiUrl}/person/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setPhotos(json);
                setIsLoading(false);
            })
            .catch(error => {
                console.error('Error fetching quotes:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch quotes')
            });
    }

    const createPhoto = async () => {
        setSelectedBase64('');

        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ Title: newPhotoTitle, PersonId: selectedPerson.id, ContentBase64: newPhotoBase64 })
        });

        if (response.ok) {
            setNewPhotoTitle('');
            setNewPhotoBase64('');
            onCallToast(0, 'New photo added');
            getPhotosByPersonId();

        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to add photo');
            onTemplateClear();
        }
    }

    const getPhotoContent = async (e) => {
        fetch(`${apiUrl}/content/${e.id}`)
            .then((res) => res.text())
            .then(text => {
                setSelectedBase64(`data:image/png;base64,${text}`);
            })
            .catch(error => {
                console.error('Error fetching photo:', error);
                onCallToast(1, 'Failed to fetch photo');
            });
    }

    useEffect(() => {
        getPhotosByPersonId();
        setNewPhotoTitle('');
        setNewPhotoBase64('');
        setSelectedBase64('');
        setSelectedPhoto();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson]);

    const handleSelect = (e) => {
        const file = e.files[0];
        const reader = new FileReader();

        reader.onloadend = () => {
            setNewPhotoBase64(reader.result.split(",")[1]);
            setNewPhotoTitle(file.name);
        };

        reader.readAsDataURL(file);
    };

    function isNewPhotoInvalid() {
        if (newPhotoBase64 === '') return true;
        if (newPhotoTitle === '') return true;

        return false;
    }

    const headerTemplate = (options) => {
        const { className, chooseButton, cancelButton } = options;

        return (
            <div className={className} style={{ backgroundColor: 'transparent', display: 'flex', alignItems: 'center', width: '100%' }}>
                {chooseButton}
                {cancelButton}
                <div className='flex w-8'>
                    <InputText className='w-10' value={newPhotoTitle} placeholder="Title" onChange={(e) => setNewPhotoTitle(e.target.value)} />
                    <Button className='ml-4' icon='pi pi-upload' aria-label="Filter" onClick={createPhoto} disabled={isNewPhotoInvalid()} />
                </div>
            </div>
        );
    };

    const itemTemplate = (file, props) => {
        return (
            <div className="flex align-items-center flex-wrap">
                <div className="flex " style={{ width: '80%' }}>
                    <img alt={file.name} role="presentation" src={file.objectURL} width={100} />
                    <span className="flex flex-column text-left ml-3">
                        {file.name}
                        <small>{new Date().toLocaleDateString()}</small>
                    </span>
                </div>
                <Tag value={props.formatSize} severity="warning" className="px-3 py-2" />
            </div>
        );
    };

    const onSelectPhoto = async (e) => {
        if (selectedPhoto) {
            setSelectedPhoto();
            setSelectedBase64('');
            return;
        }

        setSelectedPhoto(e);
        await getPhotoContent(e);
    }

    const onTemplateClear = () => {
        setNewPhotoBase64('');
        setNewPhotoTitle('');
    };

    const emptyTemplate = () => {
        return (
            <div className="flex align-items-center ">
                <i className="pi pi-image mt-3 p-5" style={{ fontSize: '5em', borderRadius: '50%', backgroundColor: 'var(--surface-b)', color: 'var(--surface-d)' }}></i>
                <span style={{ fontSize: '1.2em', color: 'var(--text-color-secondary)' }} className="my-5">
                    Drag and Drop Image Here
                </span>
            </div>
        );
    };

    const chooseOptions = { icon: 'pi pi-fw pi-images', iconOnly: true, className: 'custom-choose-btn p-button-rounded p-button-outlined' };
    const cancelOptions = { icon: 'pi pi-fw pi-times', iconOnly: true, className: 'custom-cancel-btn p-button-danger p-button-rounded p-button-outlined' };

    function renderSkeleton() {
        const items = Array.from({ length: 5 }, (v, i) => i);
        return (<DataTable value={items} className="p-datatable-striped mt-4">
            <Column field="id" header="Id" style={{ width: '5%' }} body={<Skeleton />}></Column>
            <Column field="title" header="Title" style={{ width: '75%' }} body={<Skeleton />}></Column>
        </DataTable>)
    }

    function renderPhotoItems() {
        if (isLoading) {
            return (<div className="p-field mt-4 flex">
                <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s" />
            </div>);
        } else {
            return (
                <div className='flex'>
                    <div className='col-4'>
                        <div className='text-2xl'>
                            Photos
                        </div>
                        {(photos === undefined || photos.length === 0)
                            ? renderSkeleton()
                            : (
                                <>
                                    <ContextMenu model={menuModel} ref={cm} onHide={() => onSelectPhoto(null)} />
                                    <DataTable className='mt-4' scrollable scrollHeight="400px" virtualScrollerOptions={{ itemSize: 46 }} value={photos} paginator rows={5} rowsPerPageOptions={[5, 10, 25, 50]}
                                        paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
                                        currentPageReportTemplate="{first} to {last} of {totalRecords}"
                                        selectionMode="single" selection={selectedPhoto}
                                        onSelectionChange={(e) => onSelectPhoto(e.value)}
                                        onContextMenu={(e) => cm.current.show(e.originalEvent)} contextMenuSelection={selectedPhoto} onContextMenuSelectionChange={(e) => setSelectedPhoto(e.value)} >
                                        <Column field="id" header="Id" style={{ width: '5%' }}></Column>
                                        <Column field="title" header="Title"></Column>
                                    </DataTable>
                                </>
                            )}
                    </div>
                    <div className='col-3'>
                        {selectedBase64 && (
                            <div className='flex align-items-center mt-2'>
                                <div className='flex'>
                                    <img className='w-12' style={{
                                        borderRadius: "12px",
                                        boxShadow: "0 8px 20px rgba(0,0,0,0.3)",
                                    }} src={selectedBase64} />
                                </div>
                            </div>
                        )}
                    </div>
                    <div className='col-5'>
                        <>

                            <div className='text-2xl'>
                                Add new photo
                            </div>
                            <div className='flex mt-4'>
                                <FileUpload name="demo[]" url="/api/upload" accept="image/*" maxFileSize={1000000}
                                    className='w-12'
                                    headerTemplate={headerTemplate} itemTemplate={itemTemplate} onClear={onTemplateClear} emptyTemplate={emptyTemplate}
                                    chooseOptions={chooseOptions} cancelOptions={cancelOptions} onSelect={handleSelect} />
                            </div>
                        </>
                    </div>
                </div>
            )
        }
    }

    return (
        <>
            {renderPhotoItems()}
        </>
    );
}