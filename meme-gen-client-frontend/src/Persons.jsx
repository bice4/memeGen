import { motion } from "framer-motion";
import { tr } from 'framer-motion/client';
import './App.css';

export default function Persons({ persons, handleClick }) {

    return (

        <div>
            {persons.map((item) => (
                <div style={{
                    display: "flex",
                    gap: "60px",
                    justifyContent: "center",
                    alignItems: "center",
                    height: "100vh",
                    width: "100vw - 18px",
                }}>
                    <motion.div
                        key={item.id}
                        style={{
                            width: "120px",
                            height: "120px",
                            borderRadius: "50%",
                            background: "#3b82f6", // синий
                            color: "white",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            fontWeight: "bold",
                            fontSize: "18px",
                            fontFamily: "Inter var, sans-serif",
                            cursor: "pointer",
                            boxShadow: "0 8px 20px rgba(0,0,0,0.2)",
                        }}
                        whileHover={{ scale: 1.2, rotate: 10 }}
                        whileTap={{ scale: 0.9 }}
                        animate={{ y: [0, -10, 0] }}
                        transition={{ duration: 2, repeat: Infinity, ease: "easeInOut" }}
                        onClick={() => handleClick(item.id)}
                    >
                        {item.name}
                    </motion.div>  </div>
            ))
            }</div>

    )
}
