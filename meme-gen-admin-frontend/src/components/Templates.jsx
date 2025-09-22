
import { ProgressSpinner } from 'primereact/progressspinner';
import { useState, useEffect, useRef } from "react";
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { ContextMenu } from 'primereact/contextmenu';
import { Skeleton } from 'primereact/skeleton';


export default function Templates({ selectedPerson, onCallToast }) {

    const [isLoading, setIsLoading] = useState(true);
    const [templates, setTemplate] = useState([]);
    const [selectedTemplate, setSelectedTemplate] = useState();
    const cm = useRef(null);

    const menuModel = [
        { label: 'Delete', icon: 'pi pi-fw pi-times', command: () => deleteTemplate(selectedTemplate) }
    ];

    const apiUrl = '/api/Template';

    const removeById = (id) => {
        setTemplate((prev) => prev.filter((q) => q.id !== id));
    };

    const deleteTemplate = async () => {
        const response = await fetch(`${apiUrl}/${selectedTemplate.id}`, {
            method: "DELETE",
        });

        if (response.ok) {
            onCallToast(0, 'Template deleted');
            removeById(selectedTemplate.id);
        } else {
            console.error("Error:", response.status);
            onCallToast(1, 'Failed to delete template');
        }
    };

    const getTemplates = async () => {
        if (selectedPerson == null) return;
        setIsLoading(true);
        fetch(`${apiUrl}/person/${selectedPerson.id}`)
            .then(response => response.json())
            .then(json => {
                setTemplate(json);
                setIsLoading(false);
            })
            .catch(error => {
                console.error('Error fetching templates:', error);
                setIsLoading(false);
                onCallToast(1, 'Failed to fetch templates')
            });
    }

    useEffect(() => {
        getTemplates();
        setSelectedTemplate();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedPerson]);

    function renderSkeleton() {
        const items = Array.from({ length: 5 }, (v, i) => i);

        return (<DataTable value={items} className="p-datatable-striped mt-2">
            <Column field="name" header="Name" body={<Skeleton />}></Column>
            <Column field="quotes" header="Quotes" body={<Skeleton />}></Column>
            <Column field="photoTitle" header="Photo title" body={<Skeleton />}></Column>
            <Column field="usages" header="Usages" body={<Skeleton />}></Column>
        </DataTable>)
    }

    function rendertemplate() {
        if (isLoading) {
            return (<div className="p-field mt-4 flex">
                <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="8" fill="var(--surface-ground)" animationDuration=".5s" />
            </div>);
        } else {
            return (
                <>
                    {(templates === undefined || templates.length === 0)
                        ? renderSkeleton()
                        : (
                            <>
                                <ContextMenu model={menuModel} ref={cm} onHide={() => setSelectedTemplate(null)} />
                                <DataTable className='mt-2' scrollable scrollHeight="350px" virtualScrollerOptions={{ itemSize: 46 }} value={templates} paginator rows={5} rowsPerPageOptions={[5, 10, 25, 50]}
                                    paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
                                    currentPageReportTemplate="{first} to {last} of {totalRecords}"
                                    onContextMenu={(e) => cm.current.show(e.originalEvent)} contextMenuSelection={selectedTemplate} onContextMenuSelectionChange={(e) => setSelectedTemplate(e.value)} >
                                    <Column field="name" header="Name" style={{ width: '20%' }}></Column>
                                    <Column field="quotes" header="Quotes" style={{ width: '50%' }}></Column>
                                    <Column field="photoTitle" header="Photo title" style={{ width: '20%' }}></Column>
                                    <Column field="usages" header="Usages" style={{ width: '10%' }}></Column>
                                </DataTable>
                            </>
                        )}
                </>
            );
        }
    }

    return (
        <>
            {rendertemplate()}
        </>
    );
}