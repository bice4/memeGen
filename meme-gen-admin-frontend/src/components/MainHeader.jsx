import { useState, useEffect } from 'react';
import { Dropdown } from 'primereact/dropdown';
import { Button } from 'primereact/button';


export default function MainHeader({ onSelectPerson, onCallToast, openConfiguration }) {
    const [persons, setPersons] = useState([]);
    const [selectedPerson, setSelectedPerson] = useState(null);
    const [isSelected, setIsSelected] = useState(false);

    const selectPerson = (p) => {
        if (p == undefined) {
            setIsSelected(false);
        } else {
            setIsSelected(true);
        }

        setSelectedPerson(p);
        onSelectPerson(p);
    };

    const getPersons = async () => {
        fetch("/api/Persons")
            .then(response => response.json())
            .then(json => setPersons(json))
            .catch(error => {
                console.error('Error fetching persons:', error);
                onCallToast(1, 'Failed to fetch persons');
            });
    }

    useEffect(() => {
        getPersons();
    }, []);

    return (
        <>
            {!isSelected && (
                <div className='flex flex-column align-items-center justify-content-center h-screen'>
                    <div className='text-2xl'>Quote meme gen</div>
                    <Dropdown value={selectedPerson} onChange={(e) => selectPerson(e.value)} options={persons} optionLabel="name"
                        placeholder="Select a person" className="w-full md:w-14rem w-4 mt-5" />
                </div>
            )}

            {isSelected && (
                <div className='flex align-items-center justify-content-left p-4'>
                    <span className='text-3xl'>🤠</span>
                    <Dropdown showClear value={selectedPerson} onChange={(e) => selectPerson(e.value)} options={persons} optionLabel="name"
                        placeholder="Select a person" className="w-full md:w-14rem ml-3" />
                    <Button className='ml-3' icon="pi pi-cog" rounded text severity="help" aria-label="settings" onClick={openConfiguration} />
                </div>
            )}
        </>
    );
}