import { useState, useEffect } from 'react'
import './App.css';
import Persons from './Persons';
import ImageResult from './ImageResult';

function App() {
  const [persons, setPersons] = useState([]);
  const [data, setData] = useState(null);
  const [isInitialLoading, setIsInitialLoading] = useState(true);
  const [pollingInProcess, setPollingInProcess] = useState(false);

  const maxPollTries = 5;

  const getPersons = async () => {
    setIsInitialLoading(true);
    await fetch("/api/Image")
      .then(response => response.json())
      .then(json => {
        setPersons(json);
        setIsInitialLoading(false);

      })
      .catch(error => console.error('Error fetching persons:', error));
  }
  useEffect(() => {
    getPersons();
  }, []);

  const handleTryAgain = () => {
    setData();
    setPollingInProcess(false);
    getPersons();
  }

  const handleClick = async (id) => {
    try {
      // Send request with polling
      let corId = '';
      let pollCounter = 0;

      await fetch(`/api/Image/${id}`)
        .then((res) => res.json())
        .then(json => {
          if (!json.cached) {
            corId = json.correlationId;
            setPollingInProcess(true);
          } else {
            setData({ imageBase64: json.cachedImage, status: 1 });
            setPollingInProcess(false);
            return;
          }

        })
        .catch(error => {
          console.error('Error fetching photo:', error);
          return;
        });

      // Start polling
      if (corId != '') {
        const interval = setInterval(async () => {
          try {
            const checkRes = await fetch(
              `/api/Image/poll/${corId}`
            );
            const data = await checkRes.json();

            pollCounter++;

            if (data.status !== undefined) {

              if (data.status != 0) {
                setData(data);
                clearInterval(interval);
                setPollingInProcess(false);
              }
            }

            if (pollCounter >= maxPollTries) {
              clearInterval();
              setPollingInProcess(false);
            }

          } catch (err) {
            console.error("Polling error:", err);
            setPollingInProcess(false);
          }
        }, 500);
      }

    } catch (err) {
      console.error("Error on click:", err);
      setPollingInProcess(false);
    }
  };


  function renderPollingInProcess() {
    if (pollingInProcess) {
      return (<div className="flex align-items-center justify-content-center h-screen">
        <div className="text-center p-4 border-round shadow-2 surface-card">
          <div className="text-2xl mb-4">✨ Generation in progress...</div>
          <div>
            <div className="text-lg font-bold text-primary bounce">
              🎲 Tossing pixels 🎲
            </div>
          </div>
          <div>
            <div className="text-lg font-bold text-primary pulse mb-3">
              🔤 Mixing letters 🔤
            </div>
          </div>
          <div>
            <div className="text-lg font-bold text-primary flash">
              ✨ Adding magic ✨
            </div>
          </div>
          <div className="mt-4 font-mono text-xl text-500">
            🚀 Generating
            <span className="pulse">.</span>
            <span className="pulse pulse-delay-200">.</span>
            <span className="pulse pulse-delay-400">.</span>
          </div>
        </div>
      </div>);
    }
  }

  function renderNoPersonFound() {
    if (persons.length === 0 && !isInitialLoading && !pollingInProcess) {
      return (
        <div className='flex align-items-center justify-content-center h-screen'>
          <div className=''>
            <div className='text-5xl'>🚀 Generation on the way!</div>
            <div className='flex align-items-center justify-content-center'>
              <pre style={{ fontFamily: "monospace" }}>
                {`     |
    / \\
   / _ \\
  |.o '.|
  |'._.'|
  |     |
  |     |
 /|##!##|\\
/ |##!##| \\
   (o o) 
   ( - )   < "Lift off!"
   (   )
    \`-\'`}
              </pre>
            </div>
            <div className='text-3xl text-center'>✨ Almost ready!</div>
          </div>
        </div>
      );
    }
  }

  return (
    <div>
      {(persons.length === 0 && !isInitialLoading) && (
        renderNoPersonFound()
      )}

      {(persons.length >= 0 && !data && !pollingInProcess) && (
        <Persons persons={persons} handleClick={handleClick} />)}
      {renderPollingInProcess()}
      {data && (
        <ImageResult data={data} onTryAgain={handleTryAgain} />
      )}
    </div>
  )
}

export default App
