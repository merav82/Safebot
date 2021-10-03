import tensorflow as tf
import tensorflow_hub as hub
import pymysql
import numpy as np
#import apiNoSession


def db_connection(chat_type):
    file1 = open('DBUtil', 'r')
    Lines = file1.readlines()
    db = pymysql.connect(Lines[0].strip(), Lines[1].strip(), Lines[2].strip(), chat_type)
    cursor = db.cursor()

    return (cursor,db)


def get_user_count(chatType, user):
    (cursor, db) = db_connection(chatType)
    sql = """SELECT count_malicious, count_teach_mal
          FROM %s.users
           WHERE number = %s""" %(str(chatType), user)
    cursor.execute(sql)
    user_count = cursor.fetchone()

    return user_count


def updateCountUser(chatType, user, count_user):
    (cursor, db) = db_connection(chatType)
    user_count = get_user_count(chatType,user)
    if user_count == None:
        sql = """INSERT INTO %s.users 
                    (number, count_malicious, count_teach_mal) 
                    VALUES (%s, %d, %d);""" \
              %(chatType, user, count_user[0], count_user[1])
    else:
        sql = """UPDATE %s.users
                SET count_malicious = %d,
                    count_teach_mal = %d
                WHERE number = %s"""\
                %(chatType, count_user[0],count_user[1],user)
    cursor.execute(sql)
    db.commit()


def get_all_data(cursor, dataType, chatType):
    sql = ""
    if dataType == "main_data":
        mainData = []
        sql = """SELECT * 
              FROM %s.%s
                WHERE status <> 2""" %(str(chatType), str(dataType))
        cursor.execute(sql)
        data = cursor.fetchall()
        for row in data:
            mainData.append(row)
        return mainData
    if dataType == "malicious_data":
        maliciousData = []
        sql = "SELECT * from " + str(chatType) + "." + str(dataType)
        cursor.execute(sql)
        data = cursor.fetchall()
        for row in data:
            maliciousData.append(row)
        return maliciousData
    if dataType == "criticism_data":
        criticismData = []
        sql = "SELECT * from " + str(chatType) + "." + str(dataType)
        cursor.execute(sql)
        data = cursor.fetchall()
        for row in data:
            criticismData.append(row)
        return criticismData


def get_features(message_embeddings):
    for i, message_embedding in enumerate(np.array(message_embeddings).tolist()):
        message_embedding_snippet = ", ".join(
                 (str(x) for x in message_embedding))
        vec = "[" + message_embedding_snippet + "]"
    return vec

# def make_vector(sentence):
#     if type(sentence) is str:
#         text = [sentence]
#     message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
#     vector = helper.get_features(message_embeddings)
#     vec_array = ast.literal_eval(vector)

def update_malicious_data(sentence, response, user_id, row_id, vector, chatType, original_user):
    (cursor, db) = db_connection(chatType)
    # sql_vector = "select vector_str from main_data where id = "+ str(row_id)
    # cursor.execute(sql_vector)
    # vector = cursor.fetchone()

    sql = """insert into %s.malicious_data 
          (sentence, response, tagged_by, date_added, vector_str, original_user) 
           values ("%s", "%s", %d, %s, '%s', %s);""" \
          % (chatType, sentence, response, user_id, 'sysdate()', vector, original_user)
    cursor.execute(sql)
    sql1 = 'SELECT LAST_INSERT_ID();'
    cursor.execute(sql1)
    id = cursor.fetchone()
    db.commit()
    return id[0]


def erase_data(id, chatType):
    (cursor, db) = db_connection(chatType)
    sql = "UPDATE %s.main_data SET status = %d, removal_date = %s where id = %d" \
          %(chatType, 2, 'sysdate()', id )
    cursor.execute(sql)
    db.commit()


def insert_main_data(sentence, response, user_id, taught_method, vector, chatType):
    (cursor, db) = db_connection(chatType)
    # sql_vector = "select vector_str from main_data where id = "+ str(row_id)
    # cursor.execute(sql_vector)
    # vector = cursor.fetchone()
    sql = """INSERT INTO %s.main_data 
            (sentence, response, taught_by, taught_method, taught_date, main_data.type, status ,vector_str) 
             VALUES ("%s", "%s", %d, %d, %s, 1, 1, '%s');"""\
            % (chatType, sentence, response, int(user_id), taught_method, 'sysdate()', vector)
    cursor.execute(sql)
    sql1 = 'SELECT LAST_INSERT_ID();'
    cursor.execute(sql1)
    id = cursor.fetchone()
    db.commit()
    return id[0]


def malicious_resps_num(chatType, user):
    (cursor, db) = db_connection(chatType)
    sql = """select count(*) 
             from %s.malicious_data where original_user = %s"""%(chatType, user)
    cursor.execute(sql)
    data = cursor.fetchall()
    return data[0][0]

def getGameData(game_id, chatType):
    (cursor, db) = db_connection(chatType)
    game_details = []
    sql = "select game_id, row_num, start_date, end_date, state, sub_state, " +\
                "prev_sentence, learn_sentence, problematic_resp, problematic_rowId," + \
                "sentence, response from " + chatType + ".game_details where game_id = " + game_id
    cursor.execute(sql)
    data = cursor.fetchall()
    for row in data:
        game_details.append(row)

    return game_details


def countUseSentence(rowId, chatType):
    (cursor, db) = db_connection(chatType)
    sql = """UPDATE %s.main_data 
            SET count_use = count_use+1,
                calced_count_use = calced_count_use+1
            WHERE id = %d""" %(chatType, rowId)
    cursor.execute(sql)
    db.commit()


def currentCountUse(rowId, chatType):
    (cursor, db) = db_connection(chatType)
    sql = """SELECT calced_count_use
            FROM %s.main_data
            WHERE id = %d""" %(chatType, rowId)
    cursor.execute(sql)
    current_count = cursor.fetchone()[0]
    return current_count


def updateCurrentCountUse(rowId, chatType, currentCount):
    (cursor, db) = db_connection(chatType)
    if currentCount < 10:
        sql = """UPDATE %s.main_data
                SET calced_count_use = 0
                WHERE id = %d""" %(chatType, rowId)
    else:
        sql = """UPDATE %s.main_data
                        SET calced_count_use = calced_count_use-10
                        WHERE id = %d""" % (chatType, rowId)

    cursor.execute(sql)
    db.commit()


def insertNewGame(current_game_row, chatType):
    (cursor, db) = db_connection(chatType)
    sql = """insert into %s.game_details
        (game_id, row_num, start_date, end_date, state, sub_state, prev_sentence, learn_sentence, 
        problematic_resp, problematic_rowId, sentence, response)
        values (%d, %d, "%s", %s, "%s", "%s", "%s", "%s", "%s", %d, "%s", "%s") """ \
        %(chatType, int(current_game_row["game_id"]), current_game_row["row"], current_game_row["start_date"], \
          'null', current_game_row["state"], current_game_row["sub_state"], current_game_row["prev_sentence"], \
          current_game_row["learn_sentence"], current_game_row["problematic_resp"], current_game_row["problematic_rowId"], \
          current_game_row["sentence"], current_game_row["response"])
    cursor.execute(sql)
    db.commit()


def init_rows():
    last_game_row = {}
    last_game_row["game_id"] = 0
    last_game_row["row"] = 0
    last_game_row["start_date"] = ''
    last_game_row["end_date"] = ''
    last_game_row["state"] = ''
    last_game_row["sub_state"] = ''
    last_game_row["prev_sentence"] = ''
    last_game_row["learn_sentence"] = ''
    last_game_row["problematic_resp"] = ''
    last_game_row["problematic_rowId"] = 0
    last_game_row["sentence"] = ''
    last_game_row["response"] = ''
    return last_game_row


