import re
import json
from pymongo import MongoClient
from bson import ObjectId

def parse_sql(file_path):
    parts = []
    questions = {}
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    lines = content.split('\n')
    part_id_map = {} 
    
    for line in lines:
        line = line.strip()
        if not line or line.startswith('--'):
            continue
            
        if line.startswith('INSERT INTO mcq_parts'):
            match = re.search(r"VALUES\s*\('(.*?)',\s*'(.*?)'\);", line)
            if match:
                part_name = match.group(1).replace("''", "'")
                difficulty = match.group(2).replace("''", "'")
                
                part_doc = {
                    '_id': ObjectId(),
                    'PartName': part_name,
                    'Difficulty': difficulty,
                    'CreatedAt': None 
                }
                parts.append(part_doc)
                part_id_map[len(parts)] = str(part_doc['_id'])
                
        elif line.startswith('INSERT INTO mcq_questions'):
            match = re.search(r"VALUES\s*\((\d+),\s*(\d+),\s*'(.*?)',\s*'([A-D])'\);", line)
            if match:
                q_num = int(match.group(1))
                p_id = int(match.group(2))
                q_text = match.group(3).replace("''", "'")
                correct = match.group(4)
                
                q_doc = {
                    '_id': ObjectId(),
                    'QuestionNumber': q_num,
                    'PartId': part_id_map[p_id],
                    'QuestionText': q_text,
                    'CorrectAnswer': correct,
                    'Options': [],
                    'CreatedAt': None
                }
                questions[q_num] = q_doc
                
        elif line.startswith('INSERT INTO mcq_options'):
            match = re.search(r"WHERE question_number=(\d+)\),\s*'([A-D])',\s*'(.*?)',\s*(true|false)\);", line, re.IGNORECASE)
            if match:
                q_num = int(match.group(1))
                letter = match.group(2)
                text = match.group(3).replace("''", "'")
                is_correct = match.group(4).lower() == 'true'
                
                opt_doc = {
                    'OptionLetter': letter,
                    'OptionText': text,
                    'IsCorrect': is_correct
                }
                if q_num in questions:
                    questions[q_num]['Options'].append(opt_doc)

    return parts, list(questions.values())

if __name__ == '__main__':
    from datetime import datetime, timezone
    
    parts, questions = parse_sql('e:/Mln101/game mln/datacauhoi.md')
    print(f"Parsed {len(parts)} parts and {len(questions)} questions.")
    
    now = datetime.now(timezone.utc)
    for p in parts:
        p['CreatedAt'] = now
    for q in questions:
        q['CreatedAt'] = now
        
    conn_str = "mongodb+srv://hieunguyenn1501_db_user:CeCTSdGfD3f8b0Gc@cluster0.knweend.mongodb.net/CuocDuaKyThuDb"
    client = MongoClient(conn_str)
    db = client.get_database()
    
    db.mcq_parts.delete_many({})
    db.mcq_questions.delete_many({})
    
    if parts:
        db.mcq_parts.insert_many(parts)
        print("Inserted parts.")
    if questions:
        db.mcq_questions.insert_many(questions)
        print("Inserted questions.")
        
    print("Seeding completed successfully.")
