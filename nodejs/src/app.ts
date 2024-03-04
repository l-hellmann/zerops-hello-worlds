import express from 'express';
import { connectDB } from './db';

const app = express();
const port = 3000;

app.use(express.json());

app.post('/add-entry', async (req, res) => {
  const client = await connectDB();
  const { data } = req.body;
  
  await client.query(`INSERT INTO entries(data) VALUES ($1)`, [data]);
  
  res.status(201).send({ message: 'Entry added successfully.' });
  await client.end();
});

export default app;

