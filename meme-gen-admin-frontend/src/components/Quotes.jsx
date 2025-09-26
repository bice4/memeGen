
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

export default function Quotes({ selectedPerson, onCallToast }) {

    const [isLoading, setIsLoading] = useState(true);
    const [quotes, setQuotes] = useState([]);
    const [newQuoteContent, setNewQuoteContent] = useState('');
    const [selectedQuote, setSelectedQuote] = useState();
    const [textFile, setTextFile] = useState();
    const cm = useRef(null);

    const menuModel = [
        { label: 'Delete', icon: 'pi pi-fw pi-times', command: () => deleteQuote(selectedQuote) }
    ];

    const apiUrl = '/api/Quote';

    const removeById = (id) => {
        setQuotes((prev) => prev.filter((q) => q.id !== id));
    };

    const deleteQuote = async () => {
        const response = await fetch(`${apiUrl}/${selectedQuote.id}`, {
            method: "DELETE",
        });

        if (response.ok) {
            onCallToast(0, 'Quote deleted');
            removeById(selectedQuote.id);
        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to delete quote');
        }
    };

    const getQuotes = async () => {
        if (selectedPerson == null) return;
        setIsLoading(true);
        fetch(`${apiUrl}/person/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setQuotes(json);
                setIsLoading(false);
            })
            .catch(error => {
                console.error('Error fetching quotes:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch quotes')
            });
    }

    const isQuoteValid = () => {
        return newQuoteContent !== '' && newQuoteContent.length > 50;
    }

    const uploadTextFile = async () => {
        if (textFile === undefined) return;

        const formData = new FormData();
        formData.append("file", textFile);
        formData.append("personId", selectedPerson.id);

        const response = await fetch(`${apiUrl}/file`, {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            setTextFile();
            onCallToast(0, 'Quotes from file uploaded');
            getQuotes();

        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to upload quotes from file');
            setTextFile();
        }
    }

    const handleSelect = (e) => {
        const file = e.files[0];
        if (!file) return;
        setTextFile(file);
    };

    const addNewQuote = async () => {
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ Quote: newQuoteContent, PersonId: selectedPerson.id })
        });

        if (response.ok) {
            setNewQuoteContent('');
            onCallToast(0, 'New quote added');
            getQuotes();

        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to add quote');
        }
    }

    const emptyTemplate = () => {
        return (
            <div className="flex align-items-center ">
                <i className="pi pi-align-center mt-3 p-5" style={{ fontSize: '5em', borderRadius: '50%', backgroundColor: 'var(--surface-b)', color: 'var(--surface-d)' }}></i>
                <span style={{ fontSize: '1.2em', color: 'var(--text-color-secondary)' }} className="my-5 ml-4">
                    Drag and Drop Text file Here
                </span>
            </div>
        );
    };

    const chooseOptions = { icon: 'pi pi-fw pi-images', iconOnly: true, className: 'custom-choose-btn p-button-rounded p-button-outlined' };
    const cancelOptions = { icon: 'pi pi-fw pi-times', iconOnly: true, className: 'custom-cancel-btn p-button-danger p-button-rounded p-button-outlined' };

    const itemTemplate = (file, props) => {
        return (
            <div className="flex align-items-center flex-wrap">
                <div className="flex " style={{ width: '80%' }}>
                    <span className="flex flex-column text-left ml-3">
                        {file.name}
                        <small>{new Date().toLocaleDateString()}</small>
                    </span>
                </div>
                <Tag value={props.formatSize} severity="warning" className="px-3 py-2" />
            </div>
        );
    };


    function isTextFileInvalid() {
        if (textFile === undefined) return true;
        return false;
    }

    const headerTemplate = (options) => {
        const { className, chooseButton, cancelButton } = options;

        return (
            <div className={className} style={{ backgroundColor: 'transparent', display: 'flex', alignItems: 'center', width: '100%' }}>
                {chooseButton}
                {cancelButton}
                <div className='flex w-10 flex justify-content-end'>
                    <Button className='ml-4' icon='pi pi-upload' aria-label="Filter" onClick={uploadTextFile} disabled={isTextFileInvalid()} />
                </div>
            </div>
        );
    };

    const onTemplateClear = () => {
        setTextFile();
    };

    useEffect(() => {
        getQuotes();
        setNewQuoteContent('');
        setSelectedQuote();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson]);

    function renderSkeleton() {
        const items = Array.from({ length: 5 }, (v, i) => i);

        return (<DataTable value={items} className="p-datatable-striped mt-4">
            <Column field="id" header="Id" style={{ width: '25%' }} body={<Skeleton />}></Column>
            <Column field="quote" header="Quote" style={{ width: '25%' }} body={<Skeleton />}></Column>
        </DataTable>)
    }

    function renderQuotes() {
        if (isLoading) {
            return (<div className="p-field mt-4 flex">
                <ProgressSpinner style={{ width: '50px', height: '50px' }}
                    strokeWidth="8" fill="var(--surface-ground)"
                    animationDuration=".5s" />
            </div>);
        } else return ((
            <div className='flex'>
                <div className='col-6'>
                    <div className='text-2xl'>
                        Quotes
                    </div>
                    {(quotes === undefined || quotes.length === 0)
                        ? renderSkeleton()
                        : (<>
                            <ContextMenu model={menuModel} ref={cm} onHide={() => setSelectedQuote(null)} />
                            <DataTable className='mt-4' scrollable scrollHeight="400px" virtualScrollerOptions={{ itemSize: 46 }} value={quotes} paginator rows={5} rowsPerPageOptions={[5, 10, 25, 50]}
                                paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
                                currentPageReportTemplate="{first} to {last} of {totalRecords}"
                                onContextMenu={(e) => cm.current.show(e.originalEvent)} contextMenuSelection={selectedQuote} onContextMenuSelectionChange={(e) => setSelectedQuote(e.value)} >
                                <Column field="id" header="Id" style={{ width: '5%' }}></Column>
                                <Column field="quote" header="Quote"></Column>
                            </DataTable></>)}
                </div>
                <div className='col-6'>
                    <div>
                        <div className='text-2xl'>
                            Add new quote
                        </div>
                        <div className='flex mt-4'>
                            <InputText className='w-8' invalid={isQuoteValid()} value={newQuoteContent} onChange={(e) => setNewQuoteContent(e.target.value)} />
                            <Button className='ml-4' icon='pi pi-upload' aria-label="Filter" onClick={addNewQuote} disabled={newQuoteContent === '' || isQuoteValid()} />
                        </div>

                        {isQuoteValid() && (
                            <div className='flex mt-1'>
                                <div className='text-base'>
                                    <span style={{
                                        color: "red"
                                    }}>
                                        Quote should be less or equal than 50 symbols
                                    </span>
                                </div>
                            </div>
                        )}

                        <div className='flex mt-4'>
                            <div className='text-2xl w-12'>
                                Upload text file with quotes
                                <div className='flex mt-4'>
                                    <FileUpload name="demo[]" url="/api/upload" accept="text/*" maxFileSize={1000000}
                                        className='w-12'
                                        headerTemplate={headerTemplate} itemTemplate={itemTemplate} onClear={onTemplateClear} emptyTemplate={emptyTemplate}
                                        chooseOptions={chooseOptions} cancelOptions={cancelOptions} onSelect={handleSelect} />
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        ))
    }

    return (
        <>
            {renderQuotes()}
        </>
    );
}