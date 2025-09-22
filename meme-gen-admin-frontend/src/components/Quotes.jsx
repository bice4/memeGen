
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect, useRef } from "react";
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { ContextMenu } from 'primereact/contextmenu';
import { Skeleton } from 'primereact/skeleton';

export default function Quotes({ selectedPerson, onCallToast, onCallQuotesCount }) {

    const [isLoading, setIsLoading] = useState(true);
    const [quotes, setQuotes] = useState([]);
    const [newQuoteContent, setNewQuoteContent] = useState('');
    const [selectedQuote, setSelectedQuote] = useState();
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

    useEffect(() => {
        if (quotes.length > 0) {
            onCallQuotesCount(quotes.length);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [quotes]);

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
                            <DataTable className='mt-4' scrollable scrollHeight="350px" virtualScrollerOptions={{ itemSize: 46 }} value={quotes} paginator rows={5} rowsPerPageOptions={[5, 10, 25, 50]}
                                paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
                                currentPageReportTemplate="{first} to {last} of {totalRecords}"
                                onContextMenu={(e) => cm.current.show(e.originalEvent)} contextMenuSelection={selectedQuote} onContextMenuSelectionChange={(e) => setSelectedQuote(e.value)} >
                                <Column field="id" header="Id" style={{ width: '5%' }}></Column>
                                <Column field="quote" header="Quote"></Column>
                            </DataTable></>)}
                </div>
                <div className='col-6'>
                    <>
                        <div className='text-2xl'>
                            Add new quote
                        </div>
                        <div className='flex mt-4'>
                            <InputText className='w-8' value={newQuoteContent} onChange={(e) => setNewQuoteContent(e.target.value)} />
                            <Button className='ml-4' icon='pi pi-upload' aria-label="Filter" onClick={addNewQuote} disabled={newQuoteContent === ''} />
                        </div>
                    </>
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